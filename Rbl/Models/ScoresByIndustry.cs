﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Rbl.Models
{
    [Keyless]
    public partial class ScoresByIndustry
    {
        public string IndustryCode { get; set; }
        public double? TalentScore { get; set; }
        public double? LeadershipScore { get; set; }
        public double? HrScore { get; set; }
        public double? OrganizationScore { get; set; }
        public double? OverallScore { get; set; }
    }
}