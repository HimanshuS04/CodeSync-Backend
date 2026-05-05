using CodeSync.ProjectService.Data;
using CodeSync.ProjectService.Interfaces;
using CodeSync.ProjectService.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.ProjectService.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ProjectDbContext _context;

        public CommentRepository(ProjectDbContext context)
        {
            _context = context;
        }

        public async Task<Comment> CreateAsync(
            Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<Comment?> FindByIdAsync(int id)
            => await _context.Comments
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<List<Comment>> FindByFileIdAsync(
            Guid fileId)
            => await _context.Comments
                .Where(c => c.FileId == fileId
                         && c.ParentCommentId == null)
                .Include(c => c.Replies)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

        public async Task<Comment> UpdateAsync(
            Comment comment)
        {
            comment.UpdatedAt = DateTime.UtcNow;
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task DeleteAsync(int id)
        {
            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> CountByFileAsync(
            Guid fileId)
            => await _context.Comments
                .CountAsync(c => c.FileId == fileId);
    }
}