import { useCallback, useRef } from "react";
import { Status } from "../types/game";

const statusSoundMap: Partial<Record<Status, string>> = {
  [Status.Move]: "/sounds/move.mp3",
  [Status.Capture]: "/sounds/capture.mp3",
  [Status.Promote]: "/sounds/promote.mp3",
  [Status.GameOver]: "/sounds/gameover.mp3",
};

const soundPriority = [
  Status.GameOver,
  Status.Promote,
  Status.Capture,
  Status.Move,
];

export function useGameAudio() {
  const audioCache = useRef<Record<string, HTMLAudioElement>>({});

  const getAudio = useCallback((path: string): HTMLAudioElement | null => {
    if (typeof window === "undefined" || typeof Audio === "undefined") {
      return null;
    }
    if (!audioCache.current[path]) {
      audioCache.current[path] = new Audio(path);
    }
    return audioCache.current[path];
  }, []);

  const stopAllAudio = useCallback(() => {
    Object.values(audioCache.current).forEach((audio) => {
      audio.pause();
      audio.currentTime = 0;
    });
  }, []);

  const playSoundByStatus = useCallback(
    (statusList: Status[]) => {
      const selectedStatus = soundPriority.find((status) =>
        statusList.includes(status),
      );
      const path =
        selectedStatus !== undefined ? statusSoundMap[selectedStatus] : undefined;
      if (!path) return;

      stopAllAudio();
      const audio = getAudio(path);
      if (audio) {
        audio.play().catch(() => {
          // Ignore autoplay restrictions
        });
      }
    },
    [getAudio, stopAllAudio],
  );

  const playInvalidSound = useCallback(() => {
    stopAllAudio();
    const audio = getAudio("/sounds/invalid.mp3");
    if (audio) {
      audio.play().catch(() => {
        // Ignore autoplay restrictions
      });
    }
  }, [getAudio, stopAllAudio]);

  return { playSoundByStatus, playInvalidSound };
}
