

using System.ComponentModel.DataAnnotations.Schema;

namespace Helpers.Consumer
{
    public interface IApplicationEntity
    {
        Guid ApplicationId { get; set; }
    }
}