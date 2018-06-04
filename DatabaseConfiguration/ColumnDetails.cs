using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseConfiguration
{
    public class ColumnDetails
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// Primary key to the associated table
        /// </summary>
        public int TableId { get; set; }
        /// <summary>
        /// Column name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Title for column
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Determines if column is visible
        /// </summary>
        public bool Visible { get; set; }
        /// <summary>
        /// Ordinal position
        /// </summary>
        public int OrdinalPosition { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
