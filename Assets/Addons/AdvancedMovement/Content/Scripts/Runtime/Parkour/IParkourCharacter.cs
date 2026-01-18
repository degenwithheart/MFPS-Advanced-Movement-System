using UnityEngine;
using System.Collections;

namespace FC_ParkourSystem
{
    /// <summary>
    /// Interface that character controllers must implement to use the parkour system
    /// This allows the parkour system to work with any character controller (MFPS, default Unity, custom, etc.)
    /// </summary>
    public interface IParkourCharacter
    {
        /// <summary>
        /// Should the animator use root motion during parkour actions?
        /// </summary>
        bool UseRootMotion { get; set; }

        /// <summary>
        /// Current movement direction input (normalized)
        /// </summary>
        Vector3 MoveDir { get; }

        /// <summary>
        /// Is the character currently grounded?
        /// </summary>
        bool IsGrounded { get; }

        /// <summary>
        /// Gravity value affecting the character
        /// </summary>
        float Gravity { get; }

        /// <summary>
        /// Reference to the character's animator
        /// </summary>
        Animator Animator { get; set; }

        /// <summary>
        /// Should parkour actions be prevented? (e.g., during combat, reloading, etc.)
        /// </summary>
        bool PreventParkourAction { get; }

        /// <summary>
        /// Called when a parkour action starts
        /// Should disable normal character control and prepare for animation-driven movement
        /// </summary>
        void OnStartParkourAction();

        /// <summary>
        /// Called when a parkour action ends
        /// Should re-enable normal character control
        /// </summary>
        void OnEndParkourAction();

        /// <summary>
        /// Handle vertical jump (optional implementation)
        /// </summary>
        IEnumerator HandleVerticalJump();
    }
}
