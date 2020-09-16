using ITD.PhuMyPort.DataAccess.Data;
using ITD.PhuMyPort.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ITD.PhuMyPort.DataAccess.Dao
{
 
    
    public class TransactionDao
    {
        ConfigWebContext _context = null;
        public TransactionDao(ConfigWebContext context)
        {
            _context = context;
        }
        public decimal TotalTransection()
        {
            decimal totalTran = _context.Transections.Distinct().Count();
            return totalTran;
        }


        public int TotalPlate()
        {
            var result = from a in _context.Transections
                         select a.Plate;
            var b = result.Distinct().Count();

            return b;

        }

        public int Same()
        {
            int same = 0;
            foreach (var pro in _context.Transections)
            {
                Transection tempt = new Transection();

                var a = tempt.Result = pro.Result;
                if (a == 1)
                    same++;
            }
            return same;
        }

        public int Notsame()
        {
            int notsame = 0;
            foreach (var pro in _context.Transections)
            {
                Transection tempt = new Transection();

                var a = tempt.Result = pro.Result;

                if (a == 2)
                    notsame++;
            }
            return notsame;
        }

        public int Noimg()
        {
            int noimg = 0;
            foreach (var pro in _context.Transections)
            {
                Transection tempt = new Transection();

                var a = tempt.Result = pro.Result;
                if (a == 3)
                    noimg++;
            }
            return noimg;
        }


        public int Unapproved()
        {
            int unApproved = 0;

            foreach (var pro in _context.Transections)
            {
                Transection tempt = new Transection();

                var a = tempt.Result = pro.Result;
                if (a == 0)
                    unApproved++;
            }
            return unApproved;
        }

    }
}
