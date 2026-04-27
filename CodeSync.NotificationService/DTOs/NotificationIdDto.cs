using System.ComponentModel.DataAnnotations;
namespace CodeSync.NotificationService.DTOs{
public class NotificationIdDto
{
    [Required]
    public int NotificationId{ get; set; }
}
}