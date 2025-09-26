using System;
using UnityEngine;

namespace Amanita.VScripting
{
    public class ControlWithDisplay<TDisplayEnum> : Command
    {
        [Tooltip("Display type")]
        [SerializeField] protected TDisplayEnum display;

        protected virtual bool IsDisplayNone<TEnum>(TEnum enumValue)
        {
            string displayTypeStr = Enum.GetName(typeof (TEnum), enumValue);
            return displayTypeStr == "None";
        }

        #region Public members

        public virtual TDisplayEnum Display { get { return display; } }

        #endregion
    }
}
