namespace Dxc.Shq.WebApi.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    // type: 'node',
    //size: '70*70',
    //shape: 'flow-circle',
    //color: '#FA8C16',
    //label: '起止节点',
    //x: 55,
    //y: 55,
    //id: 'ea1184e8',
    //index: 0,

    public class FTANode
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 0)]
        public int Id { get; set; }

        public string NodeName { get; set; }

        public int ParentId { get; set; }

        //[ForeignKey("FTANodeGate")]
        public int FTANodeGateId { get; set; }
        //public FTANodeGate FTANodeGate { get; set; }

        [ForeignKey("FTANodeType")]
        public int FTANodeTypeId { get; set; }
        public FTANodeType FTANodeType { get; set; }

        [Key, ForeignKey("FTAProject")]
        [Column(Order = 1)]
        public Guid FTAProjectId { get; set; }
        public FTAProject FTAProject { get; set; }

        [ForeignKey("FTANodeProperties")]
        public Guid FTANodePropertiesId { get; set; }
        public FTANodeProperties FTANodeProperties { get; set; }

        // graphic properties
        public string Size { get; set; }

        public string Shape { get; set; }

        public string Color { get; set; }

        public string Lable { get; set; }

        public double X { get; set; }
        public double Y { get; set; }

        [Column("SmallFailureRateQ")]
        public double SmallFailureRateQ { get; set; }
        public bool QValueIsModifiedByUser { get; set; } // true : user input; false: compute

        public int Index { get; set; }

        public int LayerNumber { get; set; }

        // event properties
        public string EventId { get; set; }
    }
}