namespace ProjectCrovus
{
    public interface Attackable
    {
        public void Attack<T>(T gameObject) where T : Injureable;
    }

}
