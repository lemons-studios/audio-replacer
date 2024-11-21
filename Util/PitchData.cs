namespace AudioReplacer2.Util
{
    public static class PitchData // No more evil pitch data!
    {
        // Modify THIS variable to add, remove, or modify pitch values.
        // Use multiplicative values for pitch values (for example, if you want to reduce the pitch of the recording by 4%, you would want to enter in "0.96"
        public static string[][] pitchData =
        [
            ["1.0075", "Ai Ebihara"], ["1.025", "Ayane Matsunaga"], ["0.555", "Ameno-Sagiri"], ["1.015", "Chie Satonaka"],
            ["1.012", "Chihiro Fushimi"], ["0.9925", "Daisuke Nagase"], ["1.0195", "Eri Minami"], ["1.0065", "Hanako Ohtani"],
            ["1.0", "Igor"],
            ["1.0085", "Izanami"], ["0.9970", "Kanji Tatsumi"], ["0.9875", "Kinshiro Morooka"], ["0.9975", "Kou Ichijo"],
            ["0.9825", "Kunino-Sagiri"],
            ["1.025", "Kusumi-no-Okami"], ["1.0175", "Margaret"], ["1.02", "Marie"], ["0.981", "Mitsuo Kubo"],
            ["1.03", "Nanako Dojima"],
            ["0.984", "Naoki Konishi"], ["1.0125", "Naoto Shirogane"], ["1.01575", "Noriko Kashiwagi"],
            ["0.98", "Principal (Gekkoukan)"],
            ["0.98", "Principal (Yasogami)"], ["1.01675", "Rise Kujikawa"], ["0.9845", "Ryotaro Dojima"],
            ["1.0225", "Saki Konishi"],
            ["1.015", "Sayoko Uehara"], ["1.00175", "Shu Nakajima"], ["0.9835", "Taro Namatame"], ["1", "Teddie"],
            ["0.975", "Tohru Adachi"],
            ["1.0165", "Yukiko Amagi"], ["0.9975", "Yosuke Hanamura"], ["0.9875", "Yu Narukami"], ["1.0135", "Yumi Ozawa"],
            ["1.00", "Other NPC"]
        ];

        // So for my specific use case, here are the percentage decreases and whatnot:
        // Ai Ebihara receives a 0.75% Increase
        // Ayane Matsunaga receives a 2.5% Increase
        // Ameno-Sagiri receives a 44.5% Decrease
        // Chie Satonaka receives a 1.5% Increase
        // Chihiro Fushimi receives a 1.2% Increase
        // Daisuke Nagase receives a 0.75% Decrease
        // Eri Minami receives a 1.95% Increase
        // Hanako Ohtani receives a 0.65% Increase
        // Igor does not receive a pitch change
        // Izanami receives a 0.85% Increase
        // Kanji Tatsumi receives a 0.30% Decrease
        // Kinshiro Morooka receives a 1.25% Decrease
        // Kou Ichijo receives a 0.25% Decrease
        // Kunino-Sagiri receives a 1.75% Decrease
        // Margaret receives a 1.75% Increase
        // Marie receives a 2% Increase
        // Mitsuo Kubo receives a 1.9% Decrease
        // Nanako Dojima receives a 3% Increase
        // Naoki Konishi receives a 2% Decrease
        // Naoto Shirogane receives a 1.25% Increase
        // Noriko Kashiwagi receives a 1.575% Increase
        // Gekkoukan & Yasogami Principals both get a 2% Decrease
        // Rise Kujikawa receives a 1.675% Increase
        // Ryotaro Dojima receives a 1.55% Decrease
        // Saki Konishi receives a 2.25% Increase
        // Sayoko Uehara receives a 1.5% Increase
        // Shu Nakajima receives a 0.175% Increase
        // Taro Namatame receives a 1.65% Decrease
        // Teddie does not receive a pitch change
        // Tohru Adachi receives a 2.25% Decrease
        // Yukiko Amagi receives a 1.65% Increase
        // Yosuka Hanamura receives a 0.25% Decrease
        // Yu Narukami receives a 1.25% Decrease
        // Yumi Ozawa receives a 1.35% Increase
        // Then of course there's the No pitch change option at the end
    }
}
