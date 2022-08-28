using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public enum PrivacyLevel
    {
        /// <summary>
        /// Element is public and can be seen by everyone
        /// </summary>
        Public = 0,

        /// <summary>
        /// Element is private to the mesh and can be seen
        /// by other users in the mesh
        /// </summary>
        SharedWithUsers = 2,

        /// <summary>
        /// Element is private for the user. No other users can
        /// see the item
        /// </summary>
        Private = 16
    }
}
