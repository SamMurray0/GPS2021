using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using TableAttribute = System.Data.Linq.Mapping.TableAttribute;
using ColumnAttribute = System.Data.Linq.Mapping.ColumnAttribute;

namespace GPS2021
{
    [Table(Name = "NavigationData")]
    class NavigationData
    {
        /// <summary>
        /// Primary database index
        /// </summary>
        [Column(IsPrimaryKey = true, IsDbGenerated = true)]
        public int Index;

        [Column]
        public int SensorInstance;

        [Column]
        public double Latitude;

        [Column]
        public double Longitude;

        [Column]
        public double Altitude;

        [Column]
        public double Azimuth;

        [Column]
        public double Pitch;

        [Column]
        public double Roll;

        [Column]
        public DateTime Time;

        [Column]
        public double OleDateTime;

        [Column]
        public double Velocity;
    }
}
