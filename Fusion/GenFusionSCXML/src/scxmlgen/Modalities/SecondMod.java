package scxmlgen.Modalities;

import scxmlgen.interfaces.IModality;

/**
 *
 * @author nunof
 */
public enum SecondMod implements IModality{

    G_SCREENSHOT("[Play_Pause]", 1500),
    G_SWIPE_RIGHT("[Swipe_Right]", 1500),
    G_SWIPE_LEFT("[Swipe_Left]", 1500),
    G_HANDS_AIR("[Hands_air]", 1500),
    G_KILL("[Kill]", 1500),
    G_HEADPHONES("[Headphones]", 1500),
    G_JARBAS_INIT("[Jarbas_init]", 1500);
    ;
    
    private String event;
    private int timeout;


    SecondMod(String m, int time) {
        event=m;
        timeout=time;
    }

    @Override
    public int getTimeOut() {
        return timeout;
    }

    @Override
    public String getEventName() {
        //return getModalityName()+"."+event;
        return event;
    }

    @Override
    public String getEvName() {
        return getModalityName().toLowerCase()+event.toLowerCase();
    }
    
}
