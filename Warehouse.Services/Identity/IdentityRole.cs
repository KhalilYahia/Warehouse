using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Services.Identity
{
    public class IdentityRole : IdentityRole<string>
    {
        public IdentityRole()
        {
            this.Id = Guid.NewGuid();
        }

        public IdentityRole(string name)
            : this()
        {
            this.Name = name;
        }

        public IdentityRole(string name, Guid id)
        {
            this.Name = name;
            this.Id = id;
        }

        public Guid Id { get; set; }
        


        /// <summary>
        /// Gets or sets the name for this role.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the normalized name for this role.
        /// </summary>
        public string? NormalizedName { get; set; }

        /// <summary>
        /// A random value that should change whenever a role is persisted to the store
        /// </summary>
        public string? ConcurrencyStamp { get; set; }

    }
}
