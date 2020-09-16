using ITD.PhuMyPort.DataAccess.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITD.PhuMyPort.API.ViewModels
{
    public class PLCViewModel
    {
        public IEnumerable<PLC> PLCs;
        public PLC PLC;
        public SelectList WorkplaceList;
    }
}
