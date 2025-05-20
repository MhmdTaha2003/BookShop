using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace WebApplication1
{
    public static class WC
    {
        //Web constant 

        public const string ImagePaht = @"\images\product\";

        public const string SessionInquiryId = "InquirySession";
        public const string SessionCart = "ShoopingCartSession";

        public const string AdminRole = "Admine";

        public const string CustomerRole = "Customer";

        public const string EmailAdmin = "admin.account2@gmail.com";

        public const string CategoryName = "Category";




        public const string Success = "Success";
        public const string Error = "Error";

        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusInProcess = "Processing";
        public const string StatusShipped = "Shipped";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";

        public static readonly IEnumerable<string> listStatus = new ReadOnlyCollection<string>(
           new List<string>
           {
                StatusApproved,StatusCancelled,StatusInProcess,StatusPending,StatusRefunded,StatusShipped
           });
    


}
}
