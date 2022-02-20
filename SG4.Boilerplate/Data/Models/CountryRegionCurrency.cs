﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SG4.Boilerplate.Data.Models
{
    /// <summary>
    /// Cross-reference table mapping ISO currency codes to a country or region.
    /// </summary>
    [Table("CountryRegionCurrency", Schema = "Sales")]
    [Index(nameof(CurrencyCode), Name = "IX_CountryRegionCurrency_CurrencyCode")]
    public partial class CountryRegionCurrency
    {
        /// <summary>
        /// ISO code for countries and regions. Foreign key to CountryRegion.CountryRegionCode.
        /// </summary>
        [Key]
        [StringLength(3)]
        public string CountryRegionCode { get; set; } = null!;
        /// <summary>
        /// ISO standard currency code. Foreign key to Currency.CurrencyCode.
        /// </summary>
        [Key]
        [StringLength(3)]
        public string CurrencyCode { get; set; } = null!;
        /// <summary>
        /// Date and time the record was last updated.
        /// </summary>
        [Column(TypeName = "datetime")]
        public DateTime ModifiedDate { get; set; }

        [ForeignKey(nameof(CountryRegionCode))]
        [InverseProperty(nameof(CountryRegion.CountryRegionCurrencies))]
        public virtual CountryRegion CountryRegionCodeNavigation { get; set; } = null!;
        [ForeignKey(nameof(CurrencyCode))]
        [InverseProperty(nameof(Currency.CountryRegionCurrencies))]
        public virtual Currency CurrencyCodeNavigation { get; set; } = null!;
    }
}
