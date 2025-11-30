public class TeleportState
{
    // Zmienna statyczna - wspólna dla ca³ej gry
    public static bool state = false; // Domyœlnie false (jesteœmy na górze)

    // Metody te¿ musz¹ byæ static, ¿eby odwo³ywaæ siê przez TeleportState.changeState()
    public static void changeState(bool state_)
    {
        state = state_;
    }

    public static bool get()
    {
        return state;
    }
}