"use client";

import { AppBar, Avatar, BottomNavigation, BottomNavigationAction, Box, Button, IconButton, Toolbar, Typography } from "@mui/material";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { FiCreditCard, FiHome, FiList, FiLogOut, FiMoon, FiPlus, FiSun } from "react-icons/fi";
import { useColorMode } from "../providers";
import { clearToken, getEmail } from "../lib/auth";
import Image from "next/image";
import NuevoMovimientoDialog from "./NuevoMovimientoDialog";
import AsistenteChat, { AsistenteFab } from "./AsistenteChat";

function tabOf(path: string) {
  if (path.startsWith("/movimientos")) return "movimientos";
  if (path.startsWith("/cuentas")) return "cuentas";
  return "panel";
}

export default function AppShell({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const path = usePathname();
  const { toggle, mode } = useColorMode();
  const [email, setEmail] = useState<string | null>(null);
  const [nuevoOpen, setNuevoOpen] = useState(false);
  const [chatOpen, setChatOpen] = useState(false);

  useEffect(() => {
    let cancelled = false;
    Promise.resolve().then(() => {
      if (!cancelled) setEmail(getEmail());
    });
    return () => { cancelled = true; };
  }, [path]);

  const go = (href: string) => router.push(href);
  const logout = () => {
    clearToken();
    router.push("/login");
  };

  return (
    <Box sx={{ minHeight: "100vh", display: "flex", flexDirection: "column" }}>
      <AppBar position="sticky" color="default" elevation={0} sx={{ borderBottom: 1, borderColor: "divider" }}>
        <Toolbar sx={{ gap: 1 }}>
          <Box
            component="button"
            type="button"
            aria-label="Ir al panel"
            onClick={() => go("/dashboard")}
            sx={{ p: 0, border: "none", bg: "transparent", cursor: "pointer", display: "flex", lineHeight: 0 }}
          >
            <Image
              src="/Logo.png"
              alt="Finova"
              width={44}
              height={44}
              priority
              style={{ borderRadius: 10, width: "auto", height: "clamp(36px, 6vw, 48px)" }}
            />
          </Box>
          <Box sx={{ display: { xs: "none", md: "flex" }, gap: 1, flexGrow: 1 }}>
            <Button color={tabOf(path) === "panel" ? "primary" : "inherit"} onClick={() => go("/dashboard")}>Panel</Button>
            <Button color={tabOf(path) === "movimientos" ? "primary" : "inherit"} onClick={() => go("/movimientos")}>Movimientos</Button>
            <Button color={tabOf(path) === "cuentas" ? "primary" : "inherit"} onClick={() => go("/cuentas")}>Cuentas</Button>
          </Box>
          <Box sx={{ flexGrow: 1, display: { xs: "block", md: "none" } }} />

          <Button
            variant="contained"
            startIcon={<FiPlus />}
            onClick={() => setNuevoOpen(true)}
            sx={{ display: { xs: "none", md: "inline-flex" } }}
          >
            Nuevo 
          </Button>

          <IconButton onClick={toggle} aria-label="Cambiar tema" color="inherit">
            {mode === "light" ? <FiMoon /> : <FiSun />}
          </IconButton>

          {email && (
            <>
              <Avatar sx={{ width: 32, height: 32, bgcolor: "primary.main", display: { xs: "flex" } }}>
                <Typography variant="caption" sx={{ color: "#fff", fontWeight: 700 }}>
                  {email.charAt(0).toUpperCase()}
                </Typography>
              </Avatar>
              <Typography
                variant="body2"
                color="text.secondary"
                sx={{ display: { xs: "none", sm: "block" }, maxWidth: 180, overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}
              >
                {email}
              </Typography>
            </>
          )}
          <IconButton onClick={logout} aria-label="Cerrar sesión" color="inherit">
            <FiLogOut />
          </IconButton>
        </Toolbar>
      </AppBar>

      <Box sx={{ flexGrow: 1, pb: { xs: 9, md: 3 } }}>{children}</Box>

      <BottomNavigation
        value={tabOf(path)}
        showLabels
        sx={{ display: { xs: "flex", md: "none" }, position: "fixed", bottom: 0, left: 0, right: 0, zIndex: 1100 }}
      >
        <BottomNavigationAction label="Panel" value="panel" icon={<FiHome size={22} />} onClick={() => go("/dashboard")} />
        <BottomNavigationAction label="Movim." value="movimientos" icon={<FiList size={22} />} onClick={() => go("/movimientos")} />
        <BottomNavigationAction
          value="nuevo"
          icon={
            <Avatar sx={{ bgcolor: "primary.main", width: 48, height: 48, mt: -2 }}>
              <FiPlus size={24} color="#fff" />
            </Avatar>
          }
          onClick={() => setNuevoOpen(true)}
        />
        <BottomNavigationAction label="Cuentas" value="cuentas" icon={<FiCreditCard size={22} />} onClick={() => go("/cuentas")} />
      </BottomNavigation>

      <AsistenteFab onClick={() => setChatOpen(true)} />
      <NuevoMovimientoDialog open={nuevoOpen} onClose={() => setNuevoOpen(false)} />
      <AsistenteChat open={chatOpen} onClose={() => setChatOpen(false)} />
    </Box>
  );
}
