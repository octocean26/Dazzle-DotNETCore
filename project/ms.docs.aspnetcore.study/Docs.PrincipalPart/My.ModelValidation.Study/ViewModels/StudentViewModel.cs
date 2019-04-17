using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using My.ModelValidation.Study.Data;

namespace My.ModelValidation.Study.ViewModels
{
    public class StudentViewModel:IValidatableObject
    {
        public int StudentId{ get; set; }

        [Required]
        [Remote(action: "VerifyData", controller: "Home",AdditionalFields =nameof(Age) + "," + nameof(Birthday))] //该特性必须借助前端js才能实现
        public string StudentName{ get; set; }

        [Required]
        [DataType(DataType.Date)]
        [CusStudent(2000)]
        public DateTime Birthday { get; set; }
        
        [Required]
        public int Age{ get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(Age>100)
            {
                yield return new ValidationResult("Age不能大于100", new[] { nameof(Age) });
            }
        }
    }
}
