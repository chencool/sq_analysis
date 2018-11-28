namespace Dxc.Shq.WebApi.Models
{
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
        [Key]
        public string Id { get; set; }

        public string ParentId { get; set; }

        public string Type { get; set; }

        public string Size { get; set; }

        public string Color { get; set; }

        public string Lable { get; set; }

        public double X { get; set; }
        public double Y { get; set; }

        public int Index { get; set; }

        public string EventId { get; set; }

        public double Q { get; set; }
        public double Lambada { get; set; }

        public double T { get; set; }

        public double DCrf { get; set; }
        public double DClf { get; set; }

        public double SmallQ { get; set; }
    }
}