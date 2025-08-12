
using firstProject.Iservices;
using Services.DTO;
using System.Data;


namespace Services.services
{
    public class PostsService:IPostsService
    {
        public PostsService() { }

        public int AddNewPost(AddPostDto dto)
        {
            //var post_ = new Post();
            //post_.Content = dto.Content;
            //post_.Title = dto.Title;
            //post_.BlogId = dto.BlogId;

            //using (var db = new ApplicationDbContext())
            //{
            //    db.Posts.Add(post_);  
            //    db.SaveChanges();
            //}


            //return post_.PostId;
            return 2;

        }

        public List<GetPostDto> GetAllPosts()
        {
            throw new NotImplementedException();
        }
    }
}
