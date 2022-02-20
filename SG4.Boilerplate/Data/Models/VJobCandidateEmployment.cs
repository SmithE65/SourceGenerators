﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SG4.Boilerplate.Data.Models
{
    /// <summary>
    /// Displays the content from each employement history related element in the xml column Resume in the HumanResources.JobCandidate table. The content has been localized into French, Simplified Chinese and Thai. Some data may not display correctly unless supplemental language support is installed.
    /// </summary>
    [Keyless]
    public partial class VJobCandidateEmployment
    {
        [Column("JobCandidateID")]
        public int JobCandidateId { get; set; }
        [Column("Emp.StartDate", TypeName = "datetime")]
        public DateTime? EmpStartDate { get; set; }
        [Column("Emp.EndDate", TypeName = "datetime")]
        public DateTime? EmpEndDate { get; set; }
        [Column("Emp.OrgName")]
        [StringLength(100)]
        public string? EmpOrgName { get; set; }
        [Column("Emp.JobTitle")]
        [StringLength(100)]
        public string? EmpJobTitle { get; set; }
        [Column("Emp.Responsibility")]
        public string? EmpResponsibility { get; set; }
        [Column("Emp.FunctionCategory")]
        public string? EmpFunctionCategory { get; set; }
        [Column("Emp.IndustryCategory")]
        public string? EmpIndustryCategory { get; set; }
        [Column("Emp.Loc.CountryRegion")]
        public string? EmpLocCountryRegion { get; set; }
        [Column("Emp.Loc.State")]
        public string? EmpLocState { get; set; }
        [Column("Emp.Loc.City")]
        public string? EmpLocCity { get; set; }
    }
}
