module.exports = {
  // Game mode configurations
  GAME_MODES: {
    TDM: {
      name: 'Team Deathmatch',
      minPlayers: 8,
      maxPlayers: 16,
      teamSize: 8,
      matchTimeout: 60000, // 60 seconds max wait to form match
      joinTimeout: 30000,  // 30 seconds to join after assignment
      balanced: true       // Require balanced teams
    },
    FFA: {
      name: 'Free For All',
      minPlayers: 6,
      maxPlayers: 12,
      teamSize: 1,
      matchTimeout: 45000,
      joinTimeout: 30000,
      balanced: false
    },
    CTF: {
      name: 'Capture The Flag',
      minPlayers: 10,
      maxPlayers: 20,
      teamSize: 10,
      matchTimeout: 90000,
      joinTimeout: 30000,
      balanced: true
    },
    DOM: {
      name: 'Domination',
      minPlayers: 12,
      maxPlayers: 24,
      teamSize: 12,
      matchTimeout: 90000,
      joinTimeout: 30000,
      balanced: true
    }
  },

  // Region to Photon region mapping
  PHOTON_REGIONS: {
    'US-West': 'usw',
    'US-East': 'us',
    'US-Central': 'us',
    'EU-West': 'eu',
    'EU-Central': 'eu',
    'Asia': 'asia',
    'Japan': 'jp',
    'Australia': 'au'
  },

  // Matchmaking parameters
  SKILL_RANGE: 200,        // +/- ELO for matching
  SKILL_RANGE_EXPANSION: 50, // Expand by this much every 10 seconds
  MAX_WAIT_TIME: 300,      // 5 minutes max queue time

  // Cleanup intervals
  CLEANUP_STALE_QUEUES: 600000,    // 10 minutes
  CLEANUP_STALE_MATCHES: 3600000,  // 1 hour
};
