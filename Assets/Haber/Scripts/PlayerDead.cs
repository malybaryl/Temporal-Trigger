public static class PlayerDead
{
    // Dodajemy 'static' do ka¿dego elementu
    public static bool dead = false;

    public static void set(bool dead_) 
    { 
        dead = dead_; 
    }

    public static bool get() 
    { 
        return dead; 
    } 
}
