"use client";

import { AppBar, Avatar, BottomNavigation, BottomNavigationAction, Box, Button, Dialog, DialogContent, DialogTitle, IconButton, Toolbar, Typography } from "@mui/material";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { FiCreditCard, FiHome, FiList, FiLogOut, FiMessageCircle, FiMoon, FiPlus, FiSun } from "react-icons/fi";
import { useColorMode } from "../providers";
import { clearToken, getEmail } from "../lib/auth";

function tabOf(path: string) {
  if (path.startsWith("/movimientos")) return "movimientos";
  if (path.startsWith("/cuentas")) return "cuentas";
  if (path.startsWith("/asistente")) return "asistente";
  return "panel";
}

export default function AppShell({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const path = usePathname();
  const { toggle, mode } = useColorMode();
  const [email, setEmail] = useState<string | null>(null);
  const [chooser, setChooser] = useState(false);

  useEffect(() => {
    setEmail(getEmail());
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
          <Typography
            variant="h6"
            sx={{ fontFamily: '"Sora", sans-serif', fontWeight: 700, cursor: "pointer", mr: 2 }}
            onClick={() => go("/dashboard")}
          >
            Finova
          </Typography>

          <Box sx={{ display: { xs: "none", md: "flex" }, gap: 1, flexGrow: 1 }}>
            <Button color={tabOf(path) === "panel" ? "primary" : "inherit"} onClick={() => go("/dashboard")}>Panel</Button>
            <Button color={tabOf(path) === "movimientos" ? "primary" : "inherit"} onClick={() => go("/movimientos")}>Movimientos</Button>
            <Button color={tabOf(path) === "cuentas" ? "primary" : "inherit"} onClick={() => go("/cuentas")}>Cuentas</Button>
            <Button color={tabOf(path) === "asistente" ? "primary" : "inherit"} onClick={() => go("/asistente")}>Asistente</Button>
          </Box>
          <Box sx={{ flexGrow: 1, display: { xs: "block", md: "none" } }} />

          <Button
            variant="contained"
            startIcon={<FiPlus />}
            onClick={() => setChooser(true)}
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
          onClick={() => setChooser(true)}
        />
        <BottomNavigationAction label="Cuentas" value="cuentas" icon={<FiCreditCard size={22} />} onClick={() => go("/cuentas")} />
        <BottomNavigationAction label="Ayuda" value="asistente" icon={<FiMessageCircle size={22} />} onClick={() => go("/asistente")} />
      </BottomNavigation>

      <Dialog open={chooser} onClose={() => setChooser(false)}>
        <DialogTitle>Nuevo movimiento</DialogTitle>
        <DialogContent sx={{ display: "flex", flexDirection: "column", gap: 2, minWidth: 260, pb: 3 }}>
          <Button variant="contained" color="primary" onClick={() => { setChooser(false); go("/movimientos/nuevo"); }}>
            Nuevo ingreso
          </Button>
          <Button variant="contained" color="error" onClick={() => { setChooser(false); go("/movimientos/nuevo-gasto"); }}>
            Nuevo gasto
          </Button>
        </DialogContent>
      </Dialog>
    </Box>
  );
}
