﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Rbl.Models
{
    public partial class Organization
    {
        [Key]
        public int Id { get; set; }
        [StringLength(255)]
        public string ticker { get; set; }
        [StringLength(255)]
        public string industry_code { get; set; }
        [StringLength(255)]
        public string sec_name { get; set; }
        public int? cik { get; set; }
        public bool HasExtendedData { get; set; }
    }
}