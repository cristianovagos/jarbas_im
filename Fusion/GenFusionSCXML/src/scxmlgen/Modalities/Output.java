package scxmlgen.Modalities;

import scxmlgen.interfaces.IOutput;



public enum Output implements IOutput{
    
    SCREENSHOT("[screenshot]"),
    MODO_LAZER("[lazer]"),
    MODO_TRABALHO("[trabalho]"),
    LOCK_SESSION("[lock]"),
    NEXT_TRACK("[next_track_config]"),
    PREVIOUS_TRACK("[previous_track_config]"),
    BRIGHTNESS_UP("[brightness_up_config]"),
    BRIGHTNESS_DOWN("[brightness_down_config]"),
    EMAIL_LEFT("[email_left_config]"),
    EMAIL_RIGHT("[email_right_config]"),
    CALC_LEFT("[calc_left_config]"),
    CALC_RIGHT("[calc_right_config]"),
    WEBSITE_LEFT("[website_left_config]"),
    WEBSITE_RIGHT("[website_right_config]"),
    METEO_LEFT("[meteo_left_config]"),
    METEO_RIGHT("[meteo_right_config]"),
    CAMERA_LEFT("[camera_left_config]"),
    CAMERA_RIGHT("[camera_right_config]"),
    EMAIL("[email]"),
    CALC("[calc]"),
    WEBSITE("[website]"),
    METEO("[meteo]"),
    CAMERA("[camera]"),
    SWIPE_LEFT("[Swipe_Left]"),
    SWIPE_RIGHT("[Swipe_Right]")
    ;
    
    private String event;

    Output(String m) {
        event=m;
    }
    
    public String getEvent(){
        return this.toString();
    }

    public String getEventName(){
        return event;
    }
}
