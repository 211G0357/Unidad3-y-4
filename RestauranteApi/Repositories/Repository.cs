using RestauranteApi.Models.Entities;

namespace RestauranteApi.Repositories
{
    public class Repository<T> where T : class
    {


        public HamburguesaContext Context { get; set; }

        public Repository(HamburguesaContext context)
        {
            Context = context;
        }
        //READ
        public IEnumerable<T> GetAll()
        {
            return Context.Set<T>();
        }

        public T? Get(object id)
        {
            return Context.Find<T>(id);
        }
        //CREATE
        public void Insert(T entity)
        {
            Context.Add(entity);
            Context.SaveChanges();
        }
        //UPDATE
        public void Update(T entity)
        {
            Context.Update(entity);
            Context.SaveChanges();
        }
        //DELETE
        public void Delete(object id)
        {
            var entity = Context.Find<T>(id);
            if (entity != null)
            {
                Context.Remove(entity);
                Context.SaveChanges();
            }
        }
    }
}
