using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisTarsier.Service
{
    public class StoredRecipe
    {
        [NotMapped]
        public const long NO_ID = -1;
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        public bool UserEditable { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string RecipeString { get; set; }
    }
}
