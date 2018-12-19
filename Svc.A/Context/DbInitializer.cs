namespace Svc_A.Context {
    public static class DbInitializer {
        public static void Initialize (SvcDbContext context) {
            context.Database.EnsureCreated ();

            context.SaveChanges ();
        }
    }
}
