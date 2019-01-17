/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package genfusionscxml;

import java.io.IOException;
import scxmlgen.Fusion.FusionGenerator;
import scxmlgen.Modalities.Output;
import scxmlgen.Modalities.Speech;
import scxmlgen.Modalities.SecondMod;

/**
 *
 * @author nunof
 */
public class GenFusionSCXML {

    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) throws IOException {

        FusionGenerator fg = new FusionGenerator();

        // SINGLE
        fg.Single(SecondMod.G_SWIPE_LEFT, Output.SWIPE_LEFT);
        fg.Single(SecondMod.G_SWIPE_RIGHT, Output.SWIPE_RIGHT);
        
        // REDUNDANCIAS
        fg.Redundancy(Speech.S_SCREENSHOT, SecondMod.G_SCREENSHOT, Output.SCREENSHOT);
        fg.Redundancy(Speech.S_LAZER, SecondMod.G_HANDS_AIR, Output.MODO_LAZER);
        fg.Redundancy(Speech.S_TRABALHO, SecondMod.G_KILL, Output.MODO_TRABALHO);
        fg.Redundancy(Speech.S_LOCK, SecondMod.G_JARBAS_INIT, Output.LOCK_SESSION);
        
        // COMPLEMENTARIDADE
        fg.Complementary(Speech.S_NEXT_TRACK, SecondMod.G_SWIPE_RIGHT, Output.NEXT_TRACK);
        fg.Complementary(Speech.S_PREVIOUS_TRACK, SecondMod.G_SWIPE_LEFT, Output.PREVIOUS_TRACK);
        fg.Complementary(Speech.S_BRIGHTNESS_DOWN, SecondMod.G_SWIPE_RIGHT, Output.BRIGHTNESS_DOWN);
        fg.Complementary(Speech.S_BRIGHTNESS_UP, SecondMod.G_SWIPE_LEFT, Output.BRIGHTNESS_UP);
        
        fg.Complementary(Speech.S_EMAIL, SecondMod.G_SWIPE_LEFT, Output.EMAIL_LEFT);
        fg.Complementary(Speech.S_EMAIL, SecondMod.G_SWIPE_RIGHT, Output.EMAIL_RIGHT);
        fg.Complementary(Speech.S_CALC, SecondMod.G_SWIPE_LEFT, Output.CALC_LEFT);
        fg.Complementary(Speech.S_CALC, SecondMod.G_SWIPE_RIGHT, Output.CALC_RIGHT);
        fg.Complementary(Speech.S_WEBSITE, SecondMod.G_SWIPE_LEFT, Output.WEBSITE_LEFT);
        fg.Complementary(Speech.S_WEBSITE, SecondMod.G_SWIPE_RIGHT, Output.WEBSITE_RIGHT);
        fg.Complementary(Speech.S_METEO, SecondMod.G_SWIPE_LEFT, Output.METEO_LEFT);
        fg.Complementary(Speech.S_METEO, SecondMod.G_SWIPE_RIGHT, Output.METEO_RIGHT);
        fg.Complementary(Speech.S_CAMERA, SecondMod.G_SWIPE_LEFT, Output.CAMERA_LEFT);
        fg.Complementary(Speech.S_CAMERA, SecondMod.G_SWIPE_RIGHT, Output.CAMERA_RIGHT);

        fg.Build("fusion.scxml");
    }
    
}
