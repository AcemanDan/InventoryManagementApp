using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Models
{
    public enum TransactionType
    {
        Restock,
        Sale,
        Adjustment,
        Return
    }

    /// <summary>
    /// logs every stock change (Restock / Sale / Adjustment / Return)
    /// </summary>

    public class StockTransaction
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Required]
        [Display(Name = "Transaction Type")]
        public TransactionType Type { get; set; }

        /// <summary>
        /// Positive = stock added; Negative = stock removed
        /// </summary>
        [Required]
        [Display(Name = "Quantity Change")]
        public int QuantityChange { get; set; }

        [Display(Name = "Stock After Transaction")]
        public int StockAfter { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Display(Name = "Transaction Date")]
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        [Display(Name = "Reference / PO Number")]
        public string? Reference { get; set; }

        // Navigation property
        public Product Product { get; set; } = null!;
    }
}
