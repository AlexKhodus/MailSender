//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MailSender
{
    using System;
    using System.Collections.Generic;
    
    public partial class Shablons
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Shablons()
        {
            this.Send = new HashSet<Send>();
        }
    
        public int ID { get; set; }
        public string Title { get; set; }
        public string DB_Name { get; set; }
        public string Query { get; set; }
        public string HTML { get; set; }
        public string Consumer_Type { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Send> Send { get; set; }
    }
}
