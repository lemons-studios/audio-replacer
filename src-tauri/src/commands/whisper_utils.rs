use std::fs;
use tauri::command;
use whisper_rs::{FullParams, SamplingStrategy, WhisperContext, WhisperContextParameters};

// This function is shorter than the one in the C#/WinUI version of this application and that's pretty crazy ngl
#[command]
pub fn transcribe_file(path: &str, model_path: &str) -> String {
    if !fs::metadata(model_path).is_ok() {
        return String::from("Transcription Model Unavailable");
    }

    if !fs::metadata(path).is_ok() {
        return String::from("Requested File Not Found");
    }

    let lang: &'static str = "en";
    let samples: Vec<i16> = hound::WavReader::open(path)
        .unwrap()
        .into_samples::<i16>()
        .map(|x| x.unwrap())
        .collect();

    let ctx = WhisperContext::new_with_params(model_path, WhisperContextParameters::default())
        .expect("failed to load model");

    let mut state = ctx.create_state().expect("failed to create state");
    let mut params = FullParams::new(SamplingStrategy::Greedy { best_of: 1 });
    params.set_language(Some(&lang));
    params.set_print_special(false);
    params.set_print_progress(false);
    params.set_print_realtime(false);
    params.set_print_timestamps(false);
    params.set_n_threads(calculate_n_threads(num_cpus::get() as i32));

    let mut inter_samples = vec![Default::default(); samples.len()];
    whisper_rs::convert_integer_to_float_audio(&samples, &mut inter_samples)
        .expect("failed to convert audio to float");

    // There is an error that occurs when the number of samples is odd and everything just breaks
    // mono_samples fixes that
    let mono_samples = if inter_samples.len() % 2 != 0 {
        let (even_samples, _) = inter_samples.split_at(inter_samples.len() - 1);
        whisper_rs::convert_stereo_to_mono_audio(even_samples)
            .expect("failed to convert stereo to mono")
    } else {
        whisper_rs::convert_stereo_to_mono_audio(&inter_samples)
            .expect("failed to convert stereo to mono")
    };

    state
        .full(params, &mono_samples[..])
        .expect("Model failed to run correctly");

    let num_segments = state
        .full_n_segments()
        .expect("Model failed to run correctly");

    let mut result = String::new();
    for i in 0..num_segments {
        let segment = state
            .full_get_segment_text(i)
            .expect("Model failed to run correctly");

        result.push_str(&segment);
        result.push(' ');
    }
    result.trim().to_string()
}

fn calculate_n_threads(mut n: i32) -> i32 {
    n -= 1;
    n |= n >> 1;
    n |= n >> 2;
    n |= n >> 4;
    n |= n >> 8;
    n ^ (n >> 1)
}
