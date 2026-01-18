const functions = require('firebase-functions');
const admin = require('firebase-admin');
const config = require('./config');

admin.initializeApp();
const db = admin.firestore();

// ============================================
// TRIGGER: Player joins queue
// ============================================
exports.onQueueJoin = functions.firestore
  .document('matchmaking/queues/{userId}')
  .onCreate(async (snap, context) => {
    const userId = context.params.userId;
    const queueData = snap.data();

    console.log(`[QUEUE] Player ${userId} joined:`, {
      gameMode: queueData.gameMode,
      region: queueData.region,
      skillLevel: queueData.skillLevel
    });

    try {
      // Attempt to form a match immediately
      await attemptMatchmaking(queueData.gameMode, queueData.region);
    } catch (error) {
      console.error('[ERROR] Matchmaking failed:', error);
    }
  });

// ============================================
// CORE: Attempt to form a match
// ============================================
async function attemptMatchmaking(gameMode, region) {
  const modeConfig = config.GAME_MODES[gameMode];

  if (!modeConfig) {
    console.error(`[ERROR] Invalid game mode: ${gameMode}`);
    return;
  }

  console.log(`[MATCHMAKING] Attempting for ${gameMode} in ${region}`);

  // Get all waiting players
  const queueSnapshot = await db.collection('matchmaking/queues')
    .where('gameMode', '==', gameMode)
    .where('region', '==', region)
    .where('status', '==', 'waiting')
    .orderBy('timestamp', 'asc')
    .limit(modeConfig.maxPlayers)
    .get();

  const playerCount = queueSnapshot.size;

  console.log(`[MATCHMAKING] Found ${playerCount}/${modeConfig.minPlayers} players`);

  if (playerCount < modeConfig.minPlayers) {
    console.log('[MATCHMAKING] Not enough players yet');
    return;
  }

  // Create match with available players
  const players = [];
  const queueDocs = [];

  queueSnapshot.forEach(doc => {
    players.push({
      userId: doc.id,
      skillLevel: doc.data().skillLevel,
      timestamp: doc.data().timestamp
    });
    queueDocs.push(doc);
  });

  // Balance teams if required
  let teams = null;
  if (modeConfig.balanced) {
    teams = balanceTeams(players, modeConfig.teamSize);
  }

  // Generate unique match ID and room name
  const matchId = `match_${Date.now()}_${generateRandomString(8)}`;
  const roomName = matchId;

  console.log(`[MATCH] Creating ${matchId} with ${players.length} players`);

  // Create match document
  await db.collection('matchmaking/matches').doc(matchId).set({
    roomName: roomName,
    photonRegion: config.PHOTON_REGIONS[region] || 'us',
    gameMode: gameMode,
    region: region,
    maxPlayers: modeConfig.maxPlayers,
    currentPlayers: 0,
    players: players.map(p => p.userId),
    teams: teams,
    status: 'ready',
    createdAt: admin.firestore.FieldValue.serverTimestamp(),
    joinDeadline: Date.now() + modeConfig.joinTimeout,
    startedAt: null,
    completedAt: null
  });

  // Assign match to each player
  const batch = db.batch();

  players.forEach(player => {
    const team = teams ? getPlayerTeam(player.userId, teams) : null;

    // Create player match assignment (triggers client listener)
    const playerMatchRef = db.collection('matchmaking/playerMatches').doc(player.userId);
    batch.set(playerMatchRef, {
      matchId: matchId,
      roomName: roomName,
      photonRegion: config.PHOTON_REGIONS[region] || 'us',
      gameMode: gameMode,
      team: team,
      assignedAt: admin.firestore.FieldValue.serverTimestamp(),
      joinBy: Date.now() + modeConfig.joinTimeout
    });

    // Remove from queue
    const queueRef = db.collection('matchmaking/queues').doc(player.userId);
    batch.delete(queueRef);
  });

  await batch.commit();

  console.log(`[MATCH] ${matchId} assigned to ${players.length} players`);

  // Schedule match monitoring
  setTimeout(() => monitorMatchJoin(matchId), modeConfig.joinTimeout + 5000);
}

// ============================================
// HELPER: Balance teams by skill
// ============================================
function balanceTeams(players, teamSize) {
  // Sort by skill level
  const sorted = [...players].sort((a, b) => b.skillLevel - a.skillLevel);

  const redTeam = [];
  const blueTeam = [];
  let redSkill = 0;
  let blueSkill = 0;

  // Distribute players to balance skill
  sorted.forEach(player => {
    if (redTeam.length < teamSize && (blueTeam.length === teamSize || redSkill <= blueSkill)) {
      redTeam.push(player.userId);
      redSkill += player.skillLevel;
    } else if (blueTeam.length < teamSize) {
      blueTeam.push(player.userId);
      blueSkill += player.skillLevel;
    }
  });

  console.log(`[TEAMS] Red: ${redSkill / redTeam.length} avg, Blue: ${blueSkill / blueTeam.length} avg`);

  return {
    red: redTeam,
    blue: blueTeam
  };
}

function getPlayerTeam(userId, teams) {
  if (teams.red.includes(userId)) return 'red';
  if (teams.blue.includes(userId)) return 'blue';
  return null;
}

// ============================================
// MONITOR: Check if players joined
// ============================================
async function monitorMatchJoin(matchId) {
  console.log(`[MONITOR] Checking match ${matchId}`);

  const matchDoc = await db.collection('matchmaking/matches').doc(matchId).get();

  if (!matchDoc.exists) {
    console.log(`[MONITOR] Match ${matchId} doesn't exist (already cleaned up)`);
    return;
  }

  const matchData = matchDoc.data();

  if (matchData.status === 'active') {
    console.log(`[MONITOR] Match ${matchId} is active (${matchData.currentPlayers}/${matchData.players.length})`);
    return;
  }

  // Match failed to start - cleanup
  console.log(`[MONITOR] Match ${matchId} failed to start, cleaning up`);

  await cleanupFailedMatch(matchId, matchData);
}

// ============================================
// CLEANUP: Failed match
// ============================================
async function cleanupFailedMatch(matchId, matchData) {
  const batch = db.batch();

  // Delete match
  batch.delete(db.collection('matchmaking/matches').doc(matchId));

  // Delete player assignments
  matchData.players.forEach(userId => {
    batch.delete(db.collection('matchmaking/playerMatches').doc(userId));
  });

  await batch.commit();

  console.log(`[CLEANUP] Cleaned up failed match ${matchId}`);
}

// ============================================
// TRIGGER: Player reports successful join
// ============================================
exports.onPlayerJoinedMatch = functions.https.onCall(async (data, context) => {
  if (!context.auth) {
    throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
  }

  const userId = context.auth.uid;
  const { matchId } = data;

  console.log(`[JOIN] Player ${userId} joined match ${matchId}`);

  const matchRef = db.collection('matchmaking/matches').doc(matchId);
  const matchDoc = await matchRef.get();

  if (!matchDoc.exists) {
    throw new functions.https.HttpsError('not-found', 'Match not found');
  }

  // Update match player count
  await matchRef.update({
    currentPlayers: admin.firestore.FieldValue.increment(1),
    status: 'active'
  });

  return { success: true };
});

// ============================================
// TRIGGER: Match completed
// ============================================
exports.onMatchComplete = functions.https.onCall(async (data, context) => {
  if (!context.auth) {
    throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
  }

  const { matchId, stats } = data;

  console.log(`[COMPLETE] Match ${matchId} completed`);

  // Update match status
  await db.collection('matchmaking/matches').doc(matchId).update({
    status: 'completed',
    completedAt: admin.firestore.FieldValue.serverTimestamp(),
    stats: stats
  });

  // Cleanup player assignments
  const matchDoc = await db.collection('matchmaking/matches').doc(matchId).get();
  const matchData = matchDoc.data();

  const batch = db.batch();
  matchData.players.forEach(userId => {
    batch.delete(db.collection('matchmaking/playerMatches').doc(userId));
  });
  await batch.commit();

  return { success: true };
});

// ============================================
// SCHEDULED: Cleanup stale data
// ============================================
exports.cleanupStaleData = functions.pubsub
  .schedule('every 10 minutes')
  .onRun(async (context) => {
    const now = Date.now();
    const staleThreshold = now - config.CLEANUP_STALE_QUEUES;

    // Clean old queue entries
    const staleQueues = await db.collection('matchmaking/queues')
      .where('timestamp', '<', staleThreshold)
      .get();

    const batch = db.batch();
    staleQueues.forEach(doc => batch.delete(doc.ref));
    await batch.commit();

    console.log(`[CLEANUP] Removed ${staleQueues.size} stale queue entries`);
  });

// ============================================
// UTILITIES
// ============================================
function generateRandomString(length) {
  const chars = 'abcdefghijklmnopqrstuvwxyz0123456789';
  let result = '';
  for (let i = 0; i < length; i++) {
    result += chars.charAt(Math.floor(Math.random() * chars.length));
  }
  return result;
}
