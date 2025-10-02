using System.ComponentModel.DataAnnotations;

namespace DisasterAlleviationFoundation.Models
{
    // Main Donation Record
    public class Donation
    {
        public int Id { get; set; }
        
        [Display(Name = "Donation Type")]
        public DonationType Type { get; set; }
        
        [Display(Name = "Status")]
        public DonationStatus Status { get; set; } = DonationStatus.Pledged;
        
        [Display(Name = "Donation Date")]
        public DateTime DonationDate { get; set; } = DateTime.UtcNow;
        
        public int DonorUserId { get; set; }
        public User Donor { get; set; } = null!;
        
        [Display(Name = "Anonymous Donation")]
        public bool IsAnonymous { get; set; } = false;
        
        [Display(Name = "Special Instructions")]
        public string SpecialInstructions { get; set; } = string.Empty;
        
        [Display(Name = "Pickup Required")]
        public bool RequiresPickup { get; set; } = false;
        
        [Display(Name = "Pickup Address")]
        public string PickupAddress { get; set; } = string.Empty;
        
        [Display(Name = "Preferred Pickup Date")]
        public DateTime? PreferredPickupDate { get; set; }
        
        [Display(Name = "Contact Phone")]
        [Phone]
        public string ContactPhone { get; set; } = string.Empty;
        
        [Display(Name = "Delivery Method")]
        public DeliveryMethod DeliveryMethod { get; set; }
        
        [Display(Name = "Target Area")]
        public string TargetArea { get; set; } = string.Empty;
        
        [Display(Name = "Urgency Level")]
        public UrgencyLevel UrgencyLevel { get; set; } = UrgencyLevel.Normal;
        
        // Financial Donation Properties
        [Display(Name = "Amount")]
        [DataType(DataType.Currency)]
        public decimal? Amount { get; set; }
        
        [Display(Name = "Currency")]
        public string Currency { get; set; } = "USD";
        
        [Display(Name = "Payment Method")]
        public PaymentMethod? PaymentMethod { get; set; }
        
        [Display(Name = "Transaction Reference")]
        public string TransactionReference { get; set; } = string.Empty;
        
        [Display(Name = "Tax Deductible")]
        public bool IsTaxDeductible { get; set; } = true;
        
        [Display(Name = "Receipt Required")]
        public bool ReceiptRequired { get; set; } = true;
        
        // Processing Information
        [Display(Name = "Processed Date")]
        public DateTime? ProcessedDate { get; set; }
        
        public int? ProcessedByUserId { get; set; }
        public User? ProcessedBy { get; set; }
        
        [Display(Name = "Distribution Date")]
        public DateTime? DistributionDate { get; set; }
        
        public int? DistributedByUserId { get; set; }
        public User? DistributedBy { get; set; }
        
        [Display(Name = "Notes")]
        public string Notes { get; set; } = string.Empty;
        
        // Navigation Properties
        public List<ResourceDonation> ResourceDonations { get; set; } = new();
        public List<DonationTracking> TrackingHistory { get; set; } = new();
        public List<DonationDistribution> Distributions { get; set; } = new();
    }

    // Resource Donations (Food, Clothing, Medical Supplies, etc.)
    public class ResourceDonation
    {
        public int Id { get; set; }
        public int DonationId { get; set; }
        public Donation Donation { get; set; } = null!;
        
        [Required]
        [Display(Name = "Resource Category")]
        public ResourceCategory Category { get; set; }
        
        [Required]
        [Display(Name = "Item Name")]
        public string ItemName { get; set; } = string.Empty;
        
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }
        
        [Display(Name = "Unit of Measure")]
        public string UnitOfMeasure { get; set; } = string.Empty;
        
        [Display(Name = "Estimated Value")]
        [DataType(DataType.Currency)]
        public decimal? EstimatedValue { get; set; }
        
        [Display(Name = "Condition")]
        public ItemCondition Condition { get; set; } = ItemCondition.New;
        
        [Display(Name = "Expiration Date")]
        public DateTime? ExpirationDate { get; set; }
        
        [Display(Name = "Brand")]
        public string Brand { get; set; } = string.Empty;
        
        [Display(Name = "Size/Specifications")]
        public string Size { get; set; } = string.Empty;
        
        [Display(Name = "Weight (lbs)")]
        public decimal? Weight { get; set; }
        
        [Display(Name = "Special Storage Requirements")]
        public string StorageRequirements { get; set; } = string.Empty;
        
        [Display(Name = "Allergen Information")]
        public string AllergenInfo { get; set; } = string.Empty;
        
        [Display(Name = "Status")]
        public ResourceStatus Status { get; set; } = ResourceStatus.Available;
        
        [Display(Name = "Quality Check Date")]
        public DateTime? QualityCheckDate { get; set; }
        
        public int? QualityCheckedByUserId { get; set; }
        public User? QualityCheckedBy { get; set; }
        
        [Display(Name = "Quality Notes")]
        public string QualityNotes { get; set; } = string.Empty;
    }

    // Donation Tracking
    public class DonationTracking
    {
        public int Id { get; set; }
        public int DonationId { get; set; }
        public Donation Donation { get; set; } = null!;
        
        [Display(Name = "Status")]
        public DonationStatus Status { get; set; }
        
        [Display(Name = "Status Date")]
        public DateTime StatusDate { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Location")]
        public string Location { get; set; } = string.Empty;
        
        [Display(Name = "Notes")]
        public string Notes { get; set; } = string.Empty;
        
        public int? UpdatedByUserId { get; set; }
        public User? UpdatedBy { get; set; }
        
        [Display(Name = "Estimated Delivery")]
        public DateTime? EstimatedDelivery { get; set; }
    }

    // Distribution Records
    public class DonationDistribution
    {
        public int Id { get; set; }
        public int DonationId { get; set; }
        public Donation Donation { get; set; } = null!;
        
        [Display(Name = "Distribution Date")]
        public DateTime DistributionDate { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Distribution Location")]
        public string DistributionLocation { get; set; } = string.Empty;
        
        [Display(Name = "Recipients")]
        public int NumberOfRecipients { get; set; }
        
        [Display(Name = "Recipient Organization")]
        public string RecipientOrganization { get; set; } = string.Empty;
        
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; } = string.Empty;
        
        [Display(Name = "Contact Phone")]
        [Phone]
        public string ContactPhone { get; set; } = string.Empty;
        
        public int DistributedByUserId { get; set; }
        public User DistributedBy { get; set; } = null!;
        
        [Display(Name = "Notes")]
        public string Notes { get; set; } = string.Empty;
        
        [Display(Name = "Feedback")]
        public string Feedback { get; set; } = string.Empty;
        
        public List<ResourceDistribution> ResourceDistributions { get; set; } = new();
    }

    // Individual Resource Distribution
    public class ResourceDistribution
    {
        public int Id { get; set; }
        public int DonationDistributionId { get; set; }
        public DonationDistribution DonationDistribution { get; set; } = null!;
        public int ResourceDonationId { get; set; }
        public ResourceDonation ResourceDonation { get; set; } = null!;
        
        [Display(Name = "Quantity Distributed")]
        public int QuantityDistributed { get; set; }
        
        [Display(Name = "Remaining Quantity")]
        public int RemainingQuantity { get; set; }
        
        [Display(Name = "Notes")]
        public string Notes { get; set; } = string.Empty;
    }

    // Donation Centers/Warehouses
    public class DonationCenter
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Center Name")]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;
        
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;
        
        [Display(Name = "State")]
        public string State { get; set; } = string.Empty;
        
        [Display(Name = "ZIP Code")]
        public string ZipCode { get; set; } = string.Empty;
        
        [Display(Name = "Phone")]
        [Phone]
        public string Phone { get; set; } = string.Empty;
        
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Display(Name = "Operating Hours")]
        public string OperatingHours { get; set; } = string.Empty;
        
        [Display(Name = "Capacity")]
        public int Capacity { get; set; }
        
        [Display(Name = "Current Utilization")]
        public int CurrentUtilization { get; set; }
        
        [Display(Name = "Accepted Resource Types")]
        public string AcceptedResourceTypes { get; set; } = string.Empty;
        
        [Display(Name = "Special Instructions")]
        public string SpecialInstructions { get; set; } = string.Empty;
        
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
        
        public int? ManagerUserId { get; set; }
        public User? Manager { get; set; }
    }

    // Enums
    public enum DonationType
    {
        [Display(Name = "Financial")]
        Financial,
        [Display(Name = "Resource")]
        Resource,
        [Display(Name = "Mixed")]
        Mixed
    }

    public enum DonationStatus
    {
        Pledged,
        Confirmed,
        InTransit,
        Received,
        Processing,
        QualityCheck,
        Approved,
        Rejected,
        Distributed,
        Completed,
        Cancelled
    }

    public enum ResourceCategory
    {
        [Display(Name = "Food & Beverages")]
        Food,
        [Display(Name = "Clothing & Textiles")]
        Clothing,
        [Display(Name = "Medical Supplies")]
        Medical,
        [Display(Name = "Baby & Child Care")]
        BabyChildCare,
        [Display(Name = "Personal Hygiene")]
        PersonalHygiene,
        [Display(Name = "Household Items")]
        Household,
        [Display(Name = "Electronics")]
        Electronics,
        [Display(Name = "Tools & Equipment")]
        Tools,
        [Display(Name = "Educational Materials")]
        Educational,
        [Display(Name = "Shelter Materials")]
        Shelter,
        [Display(Name = "Transportation")]
        Transportation,
        [Display(Name = "Other")]
        Other
    }

    public enum ItemCondition
    {
        [Display(Name = "New")]
        New,
        [Display(Name = "Like New")]
        LikeNew,
        [Display(Name = "Good")]
        Good,
        [Display(Name = "Fair")]
        Fair,
        [Display(Name = "Needs Repair")]
        NeedsRepair
    }

    public enum ResourceStatus
    {
        Available,
        Reserved,
        InTransit,
        Distributed,
        Expired,
        Damaged,
        Rejected
    }

    public enum DeliveryMethod
    {
        [Display(Name = "Drop-off")]
        DropOff,
        [Display(Name = "Pickup")]
        Pickup,
        [Display(Name = "Shipping")]
        Shipping,
        [Display(Name = "Direct Delivery")]
        DirectDelivery
    }

    public enum UrgencyLevel
    {
        Low,
        Normal,
        High,
        Critical,
        Emergency
    }

    public enum PaymentMethod
    {
        [Display(Name = "Credit Card")]
        CreditCard,
        [Display(Name = "Debit Card")]
        DebitCard,
        [Display(Name = "Bank Transfer")]
        BankTransfer,
        [Display(Name = "PayPal")]
        PayPal,
        [Display(Name = "Check")]
        Check,
        [Display(Name = "Cash")]
        Cash,
        [Display(Name = "Other")]
        Other
    }

    // View Models
    public class DonationViewModel
    {
        [Display(Name = "What would you like to donate?")]
        public DonationType Type { get; set; }
        
        [Display(Name = "Make this an anonymous donation")]
        public bool IsAnonymous { get; set; } = false;
        
        [Display(Name = "Special instructions or notes")]
        [DataType(DataType.MultilineText)]
        public string SpecialInstructions { get; set; } = string.Empty;
        
        [Display(Name = "Target area (if specific)")]
        public string TargetArea { get; set; } = string.Empty;
        
        [Display(Name = "How urgent is this donation?")]
        public UrgencyLevel UrgencyLevel { get; set; } = UrgencyLevel.Normal;
        
        [Display(Name = "Contact phone number")]
        [Phone]
        public string ContactPhone { get; set; } = string.Empty;
        
        // Financial Donation Fields
        [Display(Name = "Donation amount")]
        [DataType(DataType.Currency)]
        public decimal? Amount { get; set; }
        
        [Display(Name = "Payment method")]
        public PaymentMethod? PaymentMethod { get; set; }
        
        [Display(Name = "I need a tax-deductible receipt")]
        public bool ReceiptRequired { get; set; } = true;
        
        // Resource Donation Fields
        [Display(Name = "Do you need us to pick up the items?")]
        public bool RequiresPickup { get; set; } = false;
        
        [Display(Name = "Pickup address")]
        public string PickupAddress { get; set; } = string.Empty;
        
        [Display(Name = "Preferred pickup date")]
        public DateTime? PreferredPickupDate { get; set; }
        
        [Display(Name = "How will you deliver the donation?")]
        public DeliveryMethod DeliveryMethod { get; set; }
        
        // Resource Items
        public List<ResourceDonationViewModel> Resources { get; set; } = new();
    }

    public class ResourceDonationViewModel
    {
        [Required]
        [Display(Name = "Category")]
        public ResourceCategory Category { get; set; }
        
        [Required]
        [Display(Name = "Item name")]
        public string ItemName { get; set; } = string.Empty;
        
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Quantity")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;
        
        [Display(Name = "Unit")]
        public string UnitOfMeasure { get; set; } = string.Empty;
        
        [Display(Name = "Estimated value")]
        [DataType(DataType.Currency)]
        public decimal? EstimatedValue { get; set; }
        
        [Display(Name = "Condition")]
        public ItemCondition Condition { get; set; } = ItemCondition.New;
        
        [Display(Name = "Expiration date (if applicable)")]
        public DateTime? ExpirationDate { get; set; }
        
        [Display(Name = "Brand")]
        public string Brand { get; set; } = string.Empty;
        
        [Display(Name = "Size/specifications")]
        public string Size { get; set; } = string.Empty;
        
        [Display(Name = "Weight (lbs)")]
        public decimal? Weight { get; set; }
        
        [Display(Name = "Special storage requirements")]
        public string StorageRequirements { get; set; } = string.Empty;
        
        [Display(Name = "Allergen information")]
        public string AllergenInfo { get; set; } = string.Empty;
    }

    public class DonationSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public DonationType? Type { get; set; }
        public DonationStatus? Status { get; set; }
        public ResourceCategory? ResourceCategory { get; set; }
        public string? Location { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public UrgencyLevel? UrgencyLevel { get; set; }
        
        public List<Donation> Donations { get; set; } = new();
        public int TotalDonations { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class DonationDashboardViewModel
    {
        public List<Donation> RecentDonations { get; set; } = new();
        public List<Donation> MyDonations { get; set; } = new();
        public List<Donation> UrgentNeeds { get; set; } = new();
        public DonationStats Stats { get; set; } = new();
        public List<DonationCenter> NearbyDonationCenters { get; set; } = new();
        public Dictionary<ResourceCategory, int> ResourceNeeds { get; set; } = new();
    }

    public class DonationStats
    {
        public int TotalDonations { get; set; }
        public decimal TotalFinancialDonations { get; set; }
        public int TotalResourceDonations { get; set; }
        public int DonationsThisMonth { get; set; }
        public int ActiveDonations { get; set; }
        public int DistributedDonations { get; set; }
        public Dictionary<ResourceCategory, int> DonationsByCategory { get; set; } = new();
        public Dictionary<DonationStatus, int> DonationsByStatus { get; set; } = new();
        public int UniqueDonors { get; set; }
        public decimal AverageDonationAmount { get; set; }
    }

    public class DistributionViewModel
    {
        public int DonationId { get; set; }
        
        [Required]
        [Display(Name = "Distribution location")]
        public string DistributionLocation { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Number of recipients")]
        [Range(1, int.MaxValue, ErrorMessage = "Must have at least 1 recipient")]
        public int NumberOfRecipients { get; set; }
        
        [Display(Name = "Recipient organization")]
        public string RecipientOrganization { get; set; } = string.Empty;
        
        [Display(Name = "Contact person")]
        public string ContactPerson { get; set; } = string.Empty;
        
        [Display(Name = "Contact phone")]
        [Phone]
        public string ContactPhone { get; set; } = string.Empty;
        
        [Display(Name = "Distribution notes")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; } = string.Empty;
        
        public List<ResourceDistributionViewModel> ResourceDistributions { get; set; } = new();
    }

    public class ResourceDistributionViewModel
    {
        public int ResourceDonationId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int AvailableQuantity { get; set; }
        
        [Display(Name = "Quantity to distribute")]
        [Range(0, int.MaxValue)]
        public int QuantityToDistribute { get; set; }
        
        [Display(Name = "Notes")]
        public string Notes { get; set; } = string.Empty;
    }
}