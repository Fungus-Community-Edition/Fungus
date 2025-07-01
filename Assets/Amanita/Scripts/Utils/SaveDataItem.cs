// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using UnityEngine;

namespace Amanita
{
    /// <summary>
    /// A container for a single unit of saved data.
    /// The data and its associated type are stored as string properties.
    /// The data would typically be a JSON string representing a saved object.
    /// </summary>
    [System.Serializable]
    public class SaveDataItem
    {
        [SerializeField] protected string dataType = "";
        [SerializeField] protected string data = "";

        public virtual string DataType
        {
            get { return dataType; }
            set { dataType = value; }
        }

        public virtual string Data
        {
            get { return data; }
            set { data = value; }
        }

        /// <summary>
        /// Factory method to create a new SaveDataItem.
        /// </summary>
        public static SaveDataItem Create(string dataType, string data)
        {
            var item = new SaveDataItem();
            item.dataType = dataType;
            item.data = data;

            return item;
        }

        public static readonly SaveDataItem Null = new SaveDataItem()
        {
            dataType = "Null",
            data = "null"
        };
    }
}