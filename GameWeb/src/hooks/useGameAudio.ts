import { useCallback, useRef } from "react";
import { Status } from "../types/game";

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

  const playSoundByStatus = useCallback(
    (statusList: Status[]) => {
      const statusSoundMap: Partial<Record<Status, string>> = {
        [Status.Move]: "/sounds/move.mp3",
        [Status.Capture]: "/sounds/capture.mp3",
        [Status.Promote]: "/sounds/promote.mp3",
        [Status.GameOver]: "/sounds/gameover.mp3",
      };

      statusList.forEach((s) => {
        const path = statusSoundMap[s];
        if (path) {
          const audio = getAudio(path);
          if (audio) {
            audio.currentTime = 0;
            audio.play().catch(() => {
              // Ignore autoplay restrictions
            });
          }
        }
      });
    },
    [getAudio],
  );

  const playInvalidSound = useCallback(() => {
    const audio = getAudio("/sounds/invalid.mp3");
    if (audio) {
      audio.currentTime = 0;
      audio.play().catch(() => {
        // Ignore autoplay restrictions
      });
    }
  }, [getAudio]);

  return { playSoundByStatus, playInvalidSound };
}
