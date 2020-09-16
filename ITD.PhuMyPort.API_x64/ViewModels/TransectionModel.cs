using ITD.PhuMyPort.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace ITD.PhuMyPort.API_x64.ViewModels
{
    [Serializable]
    public class TransectionModel
    {
        public string Date { get; set; }
        public string Plate { get; set; }
        public string Images { get; set; }

        public int? Result { get; set; }

        public DateTime Time { get; set; }

    }
}
