using FSWebApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace FSWebApp
{
    public class FsDBContext : DbContext
    {
        public FsDBContext() : base("FsDBContext")
        {

        }
        public DbSet<FinancialStatement> FinancialStatements { get; set; }
    }
}