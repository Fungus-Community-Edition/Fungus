using System;
using System.Collections.Generic;

namespace AtMycelia.SaveSys
{
    /// <summary>
    /// Produces concrete SaveData instances for the main game state.
    /// </summary>
    public interface IMainSaveDataProducer
    {
        /// <summary>
        /// Finds all relevant sources and produces SaveData for each.
        /// </summary>
        IList<SaveData> FindAndCreateAll(Action<IList<SaveData>> onComplete = null);
    }
}