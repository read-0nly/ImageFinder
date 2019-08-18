using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ImageComparer
{
    [Serializable]
    class CurrentSearch
    {
        public string targetFolder = "";
        public string rootFolder = "";
        public Dictionary<String, Image> targetFiles = new Dictionary<String, Image>();
        public Dictionary<String, List<float>> rootFiles = new Dictionary<String, List<float>>();
        public Dictionary<String, List<float>> currentSearch = new Dictionary<String, List<float>>();
    }
}
