using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NETVisualizer
{
    /// <summary>
    /// A simple helper class to compute the current frame rate
    /// </summary>
    internal class FPSCounter
    {
        /// <summary>
        /// Cumulative time (cumulates up to one second)
        /// </summary>
        static double _time = 0.0;

        /// <summary>
        /// Cumulative frames
        /// </summary>
        static double _frames = 0.0;

        /// <summary>
        /// Current frame rate
        /// </summary>
        static int _fps = 0;

        /// <summary>
        /// Computes and returns the current frame rate based on the time elapsed since the last frame
        /// </summary>
        /// <param name="time">The time elapsed since the last frame in seconds</param>
        /// <returns>The current frames per second</returns>
        public static int GetFps(double time)
        {
            _time += time;

            // Add up elapsed time and rendered frames up to one second
            if (_time < 1.0)
            {
                // Count the number of rendered frames
                _frames++;
                
                // Return the frames per second computed for the previous second
                return _fps;
            }
            else // A full second has elapsed ... 
            {
                // Store number of rendered frames as frame rate
                _fps = (int)_frames;

                // Reset cumulative time and frames
                _time = 0.0;
                _frames = 0.0;

                // Return frame rate for last second
                return _fps;
            }
        }
    }
}
