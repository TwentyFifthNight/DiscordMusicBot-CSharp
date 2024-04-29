using Microsoft.EntityFrameworkCore;
using Satescuro.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satescuro.Database.Context
{
	public class MyDbContext : DbContext
	{
		public DbSet<PlayerEntity> Players { get; set; }

		public MyDbContext(DbContextOptions options) :
			base(options)
		{
		}
	}
}
