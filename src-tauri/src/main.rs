#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]
use velopack::*;
fn main() {
    VelopackApp::build().run();
    audio_replacer_lib::run()
}
