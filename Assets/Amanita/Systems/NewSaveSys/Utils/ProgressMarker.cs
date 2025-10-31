using UnityEngine;
using System;

namespace Amanita.SaveSys
{
    [Serializable]
    public class ProgressMarker
    {
        [SerializeField] protected string id = string.Empty;
        [SerializeField] protected int order = 0;

        public ProgressMarker(string id, int order)
        {
            this.id = id;
            this.order = order;
        }

        public virtual string Id
        {
            get { return id; }
        }

        /// <summary>
        /// Affects the order of the execution of the blocks set to execute in response 
        /// to the game being loaded with this progress marker active. Lower number
        /// means earlier execution.
        /// </summary>
        public virtual int Order
        {
            get { return order; }
            set { order = value; }
        }

    }
}