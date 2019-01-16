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
        public int Id { get; set; }

        public string Name { get; set; }

        public int ParentId { get; set; }

        //[ForeignKey("FTANoteGate")]
        public int FTANoteGateId { get; set; }
        //public FTANoteGate FTANoteGate { get; set; }

        [ForeignKey("FTANoteType")]
        public int FTANoteTypeId { get; set; }
        public FTANoteType FTANoteType { get; set; }

        [ForeignKey("FTAProject")]
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

        public int Index { get; set; }

        // event properties
        public string EventId { get; set; }
    }
}