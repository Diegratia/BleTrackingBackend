using System;

namespace Data.ViewModels
{
    public class BleReaderNodeDto
    {
        public long Generate { get; set; }
        public Guid Id { get; set; }
        public Guid ReaderId { get; set; }
        // public int StartPos { get; set; }
        // public int EndPos { get; set; }
        
        public string StartPos { get; set; }
        public string EndPos { get; set; }        
        
        public float DistancePx { get; set; }
        public float Distance { get; set; }
        public Guid ApplicationId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public MstBleReaderDto Reader { get; set; }
        
    }

       public class BleReaderNodeCreateDto
    {
        public Guid ReaderId { get; set; }
        // public int StartPos { get; set; }
        // public int EndPos { get; set; }
        public string StartPos { get; set; }
        public string EndPos { get; set; }     
        public float DistancePx { get; set; }
        public float Distance { get; set; }
        public Guid ApplicationId { get; set; }

    }

       public class BleReaderNodeUpdateDto
    {
        public Guid ReaderId { get; set; }
        // public int StartPos { get; set; }
        // public int EndPos { get; set; }
        public string StartPos { get; set; }
        public string EndPos { get; set; }     
        public float DistancePx { get; set; }
        public float Distance { get; set; }
        public Guid ApplicationId { get; set; }

    }

    
}