"use client";

import { ThemeProvider, CssBaseline, createTheme } from "@mui/material";
import { createContext, useCallback, useContext, useMemo, useState } from "react";

type Mode = "light" | "dark";

const ColorModeContext = createContext({ toggle: () => {}, mode: "light" as Mode });

export function useColorMode() {
  return useContext(ColorModeContext);
}

// Bosque y moneda — modo claro
const light = {
  ink: "#12211B",
  paper: "#F4F6F2",
  esmeralda: "#0E7A5F",
  oro: "#C99A2C",
  rojo: "#C0392B",
  niebla: "#E3E8E3",
};

// Modo oscuro — opuestos
const dark = {
  ink: "#F4F6F2",
  paper: "#12211B",
  esmeralda: "#3AA586",
  oro: "#C99A2C",
  rojo: "#E06C5B",
  niebla: "#2A3B33",
};

function buildTheme(mode: Mode) {
  const c = mode === "light" ? light : dark;
  return createTheme({
    palette: {
      mode,
      primary: { main: c.esmeralda },
      secondary: { main: c.oro },
      error: { main: c.rojo },
      background: { default: c.paper, paper: mode === "light" ? "#FFFFFF" : "#1A2A23" },
      text: { primary: c.ink, secondary: mode === "light" ? "#4A5A52" : "#A9B8B0" },
      divider: c.niebla,
    },
    typography: {
      fontFamily: '"Inter", "Roboto", "Helvetica", "Arial", sans-serif',
      h1: { fontFamily: '"Sora", "Inter", sans-serif' },
      h2: { fontFamily: '"Sora", "Inter", sans-serif' },
      h3: { fontFamily: '"Sora", "Inter", sans-serif' },
      h4: { fontFamily: '"Sora", "Inter", sans-serif' },
      h5: { fontFamily: '"Sora", "Inter", sans-serif' },
      h6: { fontFamily: '"Sora", "Inter", sans-serif' },
    },
    components: {
      MuiBottomNavigation: {
        styleOverrides: { root: { backgroundColor: "#12211B" } },
      },
      MuiBottomNavigationAction: {
        styleOverrides: {
          root: {
            color: "#8AA098",
            "&.Mui-selected": { color: "#3AA586" },
          },
        },
      },
    },
  });
}

export default function Providers({ children }: { children: React.ReactNode }) {
  const [mode, setMode] = useState<Mode>(() => {
    if (typeof window === "undefined") return "light";
    return localStorage.getItem("finova_mode") === "dark" ? "dark" : "light";
  });

  const toggle = useCallback(() => {
    setMode((m) => {
      const next = m === "light" ? "dark" : "light";
      localStorage.setItem("finova_mode", next);
      return next;
    });
  }, []);

  const theme = useMemo(() => buildTheme(mode), [mode]);

  return (
    <ColorModeContext.Provider value={{ toggle, mode }}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        {children}
      </ThemeProvider>
    </ColorModeContext.Provider>
  );
}
