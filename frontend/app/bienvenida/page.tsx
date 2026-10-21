"use client";

import { Box, Button, Container, Typography, Divider, Link, ThemeProvider } from "@mui/material";
import {
  MdAccountBalanceWallet,
  MdReceiptLong,
  MdFilterList,
  MdDashboard,
  MdDonutSmall,
  MdCompareArrows,
  MdPriceCheck,
} from "react-icons/md";
import { FiMail } from "react-icons/fi";
import { FaGithub, FaLinkedin } from "react-icons/fa";
import { useRouter } from "next/navigation";
import Image from "next/image";
import { darkTheme } from "../providers";

export default function BienvenidaPage() {
  const router = useRouter();
  const go = (h: string) => router.push(h);

  return (
    <ThemeProvider theme={darkTheme}>
    <Box sx={{ bgcolor: "#030303", color: "#F4F6F2", minHeight: "100vh" }}>
     
    {/* HEADER */}
    <Box
      component="header"
      sx={{
        bgcolor: "#030303",
        color: "#F4F6F2",
        borderBottom: "1px solid #2A3B33",
        py: 2,
      }}
    >
      <Container maxWidth="lg">
        <Box
          sx={{
            display: "flex",
            alignItems: "center",
            justifyContent: "space-between",
            gap: 2,
          }}
        >
           {/* Logo a la derecha */}
         <Image
              src="/Logo.png"
              alt="Gráfico de Finova"
              width={600}
              height={500}
              style={{
                width: "100%",
                height: "auto",
                maxWidth: "100px",
              }}
            />
          {/* Enlaces a la izquierda */}
          <Box
            component="nav"
            sx={{
              display: "flex",
              alignItems: "center",
              gap: 3,
            }}
          >
            <Link href="#funcionalidades" underline="none" sx={{ color: "inherit", display: { xs: "none", md: "inline" } }}>
              Funcionalidades
            </Link>

            <Link href="#preguntas-frecuentes" underline="none" sx={{ color: "inherit", display: { xs: "none", md: "inline" } }}>
              Preguntas frecuentes
            </Link>

            <Link href="#contacto" underline="none" sx={{ color: "inherit", display: { xs: "none", md: "inline" } }}>
              Contacto
            </Link>

            <Button
              variant="contained"
              onClick={() => go("/registro")}
              sx={{
                bgcolor: "#3AA586",
                color: "#030303",
                textTransform: "none",
                fontWeight: 600,
                borderRadius: 2,
                "&:hover": { bgcolor: "#2D8A6D" },
              }}
            >
              Pedir acceso
            </Button>
          </Box>

         
        </Box>
      </Container>
    </Box>

      {/* HERO */}
      <Box sx={{ bgcolor: "#030303", color: "#F4F6F2", py: { xs: 6, md: 10 } }}>
        <Container maxWidth="lg">
          <Box
            sx={{
              display: "flex",
              flexDirection: { xs: "column", md: "row" },
              gap: 5,
              alignItems: "center",
            }}
          >
            <Box sx={{ flex: 1, minWidth: 0 }}>
              <Typography
                variant="h6"
                sx={{
                  fontFamily: '"Sora", sans-serif',
                  color: "Gray",
                  fontWeight: 700,
                  mb: 2,
                }}
              >
                Finanzas personales
              </Typography>
              <Typography
                variant="h4"
                component="h1"
                sx={{
                  fontFamily: '"Sora", sans-serif',
                  fontWeight: 700,
                  mb: 2,
                }}
              >
                Todas tus cuentas, un solo saldo real
              </Typography>
              <Typography variant="body1" sx={{ mb: 3, opacity: 0.85 }}>
                Finova registra tus ingresos y gastos en las cuentas que ya usás
                (banco, Mercado Pago, efectivo) y te muestra cuánto tenés,
                cuánto entró y en qué se te fue la plata.
              </Typography>
              <Button
                variant="contained"
                color="primary"
                size="large"
                sx={{ borderRadius: 3 }}
                onClick={() => go("/registro")}
              >
                Pedir acceso
              </Button>
            </Box>

            {/* grafico.png tiene 2 gráficos lado a lado: se corta en 2 mitades apiladas */}
            <Box
              sx={{
                flex: 1,
                minWidth: 0,
                width: "100%",
                display: "flex",
                flexDirection: "column",
                gap: 2,
              }}
            >
              <Box sx={{ overflow: "hidden", borderRadius: 2, lineHeight: 0 }}>
                <Image
                  src="/grafico.png"
                  alt="Gráfico de Finova"
                  width={1497}
                  height={338}
                  priority
                  style={{ width: "200%", maxWidth: "none", height: "auto" }}
                />
              </Box>
              <Box sx={{ overflow: "hidden", borderRadius: 2, lineHeight: 0 }}>
                <Image
                  src="/grafico.png"
                  alt="Gráfico de Finova"
                  width={1497}
                  height={338}
                  style={{ width: "200%", maxWidth: "none", height: "auto", marginLeft: "-100%" }}
                />
              </Box>
            </Box>
          </Box>
        </Container>
      </Box>

      {/* Núcleo */}
      <Container id="funcionalidades" maxWidth="md" sx={{ py: 6, scrollMarginTop: 10 }}>
        <Typography
          variant="h5"
          sx={{
            color: "grey",
            fontFamily: '"Sora", sans-serif',
            fontWeight: 700,
            mb: 2,
          }}
        >
          NÚCLEO
        </Typography>
        <Typography variant="h2" color="text.secondary" sx={{ fontSize: { xs: "2rem", md: "3.75rem" } }}>
          Cargá, mirá y sabé <br /> cómo estás parado
        </Typography>
        <Typography variant="body1" sx={{ mb: 3, opacity: 0.85 }}>
          Cuentas, movimientos, historial y panel del período. Es el día a día
          de tus finanzas, en un solo lugar y sin planillas.
        </Typography>
      </Container>

      {/* Núcleo*/}
      <Box
        sx={{
          display: "flex",
          flexDirection: { xs: "column", md: "row" },
          gap: 2,
          px: { xs: 2, sm: 4, md: 6 },
        }}
      >
        {" "}
        <Box
          sx={{
            background: "#12211B",
            border: "2px solid #2A3B33",
            borderRadius: 3,
            p: 3,
          }}
        >
          <MdAccountBalanceWallet size={40} color="green" />

          <Typography
            variant="h4"
            sx={{ fontFamily: '"Sora", sans-serif', fontWeight: 700, pt: 3 }}
          >
            Cuentas con su saldo
          </Typography>

          <Typography
            variant="h6"
            sx={{
              fontFamily: '"Sora", sans-serif',
              fontWeight: 700,
              textTransform: "capitalize",
            }}
          >
            Banco, Mercado Pago o efectivo, cada una con su nombre, su moneda y
            su saldo al día.
          </Typography>
        </Box>
        <Box
          sx={{
            background: "#12211B",
            border: "2px solid #2A3B33",
            borderRadius: 3,
            p: 3,
          }}
        >
          <MdReceiptLong size={40} color="yellow" />
          <Typography
            variant="h4"
            sx={{ fontFamily: '"Sora", sans-serif', fontWeight: 700, pt: 3 }}
          >
            Ingresos y gastos
          </Typography>
          <Typography
            variant="h6"
            sx={{
              fontFamily: '"Sora", sans-serif',
              fontWeight: 700,
              textTransform: "capitalize",
            }}
          >
            Cargás el monto, la fecha, la cuenta y la categoría. El saldo se
            actualiza solo.
          </Typography>
        </Box>
        <Box
          sx={{
            background: "#12211B",
            border: "2px solid #2A3B33",
            borderRadius: 3,
            p: 3,
          }}
        >
          <MdFilterList size={40} color="green" />
          <Typography
            variant="h4"
            sx={{ fontFamily: '"Sora", sans-serif', fontWeight: 700, pt: 3 }}
          >
            Historial filtrable
          </Typography>
          <Typography
            variant="h6"
            sx={{
              fontFamily: '"Sora", sans-serif',
              fontWeight: 700,
              textTransform: "capitalize",
            }}
          >
            Banco, Mercado Pago o efectivo, cada una con su nombre, su moneda y
            su saldo al día.
          </Typography>
        </Box>
        <Box
          sx={{
            background: "#12211B",
            border: "2px solid #2A3B33",
            borderRadius: 3,
            p: 3,
          }}
        >
          <MdDashboard size={40} color="green" />
          <Typography
            variant="h4"
            sx={{ fontFamily: '"Sora", sans-serif', fontWeight: 700, pt: 3 }}
          >
            Panel del período
          </Typography>
          <Typography
            variant="h6"
            sx={{
              fontFamily: '"Sora", sans-serif',
              fontWeight: 700,
              textTransform: "capitalize",
            }}
          >
            Saldo total, cuánto entró, cuánto salió y los últimos movimientos.
          </Typography>
        </Box>
      </Box>

      {/* Panel */}
      <Container maxWidth="md" sx={{ py: 6 }}>
        <Typography
          variant="h5"
          sx={{
            color: "grey",
            fontFamily: '"Sora", sans-serif',
            fontWeight: 700,
            mb: 2,
          }}
        >
          PANEL
        </Typography>
        <Typography variant="h2" color="text.secondary" sx={{ fontSize: { xs: "2rem", md: "3.75rem" } }}>
          Todo lo que importa, <br /> en una sola pantalla
        </Typography>
        <Typography variant="body1" sx={{ mb: 3, opacity: 0.85 }}>
          Al entrar ves el saldo total sumando todas tus cuentas, cuánto entró y
          cuánto salió en el período, y los últimos movimientos. Sin abrir cinco
          apps ni armar una planilla.
        </Typography>
      </Container>

      {/* Análisis */}
      <Container maxWidth="md" sx={{ py: 6 }}>
        <Typography
          variant="h5"
          sx={{
            color: "grey",
            fontFamily: '"Sora", sans-serif',
            fontWeight: 700,
            mb: 2,
          }}
        >
          ANÁLISIS
        </Typography>
        <Typography variant="h2" color="text.secondary" sx={{ fontSize: { xs: "2rem", md: "3.75rem" } }}>
          Cuando ya hay datos, <br /> Finova los mira por vos
        </Typography>
        <Typography variant="body1" sx={{ mb: 3, opacity: 0.85 }}>
          Con los movimientos cargados, Finova te muestra dónde se va la plata y
          te ayuda a ponerle un techo.
        </Typography>
      </Container>
      <Box
        sx={{
          display: "flex",
          flexDirection: { xs: "column", md: "row" },
          gap: 2,
          px: { xs: 2, sm: 4, md: 6 },
        }}
      >
        <Box
          sx={{
            background: "#12211B",
            border: "2px solid #2A3B33",
            borderRadius: 3,
            p: 3,
          }}
        >
          <MdDonutSmall size={40} color="green" />
          <Typography
            variant="h4"
            sx={{ fontFamily: '"Sora", sans-serif', fontWeight: 700, pt: 3 }}
          >
            Gastos por categoría
          </Typography>
          <Typography
            variant="h6"
            sx={{
              fontFamily: '"Sora", sans-serif',
              fontWeight: 700,
              textTransform: "capitalize",
            }}
          >
            Mirás qué porcentaje se fue en cada categoría y cuál pesa más en tu
            mes.
          </Typography>
        </Box>

        <Box
          sx={{
            background: "#12211B",
            border: "2px solid #2A3B33",
            borderRadius: 3,
            p: 3,
          }}
        >
          <MdCompareArrows size={40} color="green" />
          <Typography
            variant="h4"
            sx={{ fontFamily: '"Sora", sans-serif', fontWeight: 700, pt: 3 }}
          >
            Ingresos vs. gastos
          </Typography>
          <Typography
            variant="h6"
            sx={{
              fontFamily: '"Sora", sans-serif',
              fontWeight: 700,
              textTransform: "capitalize",
            }}
          >
            Comparás lo que entró contra lo que salió y cómo evolucionó tu
            saldo.
          </Typography>
        </Box>

        <Box
          sx={{
            background: "#12211B",
            border: "2px solid #2A3B33",
            borderRadius: 3,
            p: 3,
          }}
        >
          <MdPriceCheck size={40} color="green" />
          <Typography
            variant="h4"
            sx={{ fontFamily: '"Sora", sans-serif', fontWeight: 700, pt: 3 }}
          >
            Presupuestos con aviso
          </Typography>
          <Typography
            variant="h6"
            sx={{
              fontFamily: '"Sora", sans-serif',
              fontWeight: 700,
              textTransform: "capitalize",
            }}
          >
            Ponés un límite por categoría y Finova te avisa cuando te estás por
            pasar.
          </Typography>
        </Box>

        <Box
          sx={{
            background: "#12211B",
            border: "2px solid #2A3B33",
            borderRadius: 3,
            p: 3,
          }}
        >
          <MdDashboard size={40} color="green" />
          <Typography
            variant="h4"
            sx={{ fontFamily: '"Sora", sans-serif', fontWeight: 700, pt: 3 }}
          >
            Panel del período
          </Typography>
          <Typography
            variant="h6"
            sx={{
              fontFamily: '"Sora", sans-serif',
              fontWeight: 700,
              textTransform: "capitalize",
            }}
          >
            Saldo total, cuánto entró, cuánto salió y los últimos movimientos.
          </Typography>
        </Box>
      </Box>

      {/* ASISTENTE*/}
      <Box sx={{ bgcolor: "#030303", color: "#F4F6F2", py: { xs: 6, md: 10 } }}>
        <Container maxWidth="md">
          <Box
            sx={{
              display: "flex",
              flexDirection: { xs: "column", md: "row" },
              gap: 4,
              alignItems: "center",
            }}
          >
            <Image
              src="/Asistente.png"
              alt="Gráfico de Finova"
              width={600}
              height={500}
              style={{
                width: "100%",
                height: "auto",
                maxWidth: "600px",
              }}
            />
            <Box sx={{ minWidth: 0, transform: { xs: "none", md: "translateY(-125px)" } }}>
              <Typography
                variant="h6"
                sx={{
                  fontFamily: '"Sora", sans-serif',
                  color: "Gray",
                  fontWeight: 700,
                  mb: 2,
                }}
              >
                Asistente
              </Typography>
              <Typography
                variant="h4"
                component="h1"
                sx={{
                  fontFamily: '"Sora", sans-serif',
                  fontWeight: 700,
                  mb: 2,
                }}
              >
                Preguntale a tus finanzas <br />
                en vez de buscarlas
              </Typography>
              <Typography variant="body1" sx={{ mb: 3, opacity: 0.85 }}>
                Cuánto gasté este mes, en qué gasté más, cuánto tengo en total,
                si gasté más que el mes pasado. El asistente responde con tus
                movimientos reales, no con estimaciones.
              </Typography>
            </Box>
          </Box>
        </Container>
      </Box>

      {/* Cómo empezar */}
      <Container maxWidth="md" sx={{ py: 6 }}>
        <Typography
          variant="h5"
          sx={{
            color: "grey",
            fontFamily: '"Sora", sans-serif',
            fontWeight: 700,
            mb: 2,
          }}
        >
          CÓMO EMPEZAR
        </Typography>
        <Typography variant="h2" color="text.secondary" sx={{ fontSize: { xs: "2rem", md: "3.75rem" } }}>
          Tres pasos y ya <br /> estás registrando
        </Typography>
        <Typography variant="body1" sx={{ mb: 3, opacity: 0.85 }}>
          No hay que conectar el homebanking ni importar nada de otro sistema.
          Arrancás cargando lo que tenés hoy y seguís desde ahí.
        </Typography>
      </Container>

      {/* Pasos para empezar */}
      <Box
        sx={{
          position: "relative",
          display: "grid",
          gridTemplateColumns: {
            xs: "1fr",
            md: "repeat(3, minmax(0, 1fr))",
          },
          gap: 4,
          px: { xs: 2, sm: 4, md: 6 },
        }}
      >
        {/* Línea horizontal: solo en pantallas medianas o grandes */}
        <Box
          sx={{
            display: { xs: "none", md: "block" },
            position: "absolute",
            top: "20px",
            left: "6.66%",
            right: "6.66%",
            height: "2px",
            bgcolor: "divider",
          }}
        />

        {/* Paso 1 */}
        <Box sx={{ position: "relative" }}>
          <Box
            sx={{
              width: 40,
              height: 40,
              borderRadius: "50%",
              border: "1px solid #2A3B33",
              borderColor: "primary.main",
              bgcolor: "#12211B",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              fontWeight: 700,
              position: "relative",
              zIndex: 1,
            }}
          >
            1
          </Box>

          <Typography
            variant="h5"
            sx={{
              fontFamily: '"Sora", sans-serif',
              fontWeight: 700,
              mt: 3,
              mb: 1,
            }}
          >
            Creá tu usuario
          </Typography>

          <Typography variant="body1">
            Te registrás con tu email y listo. Cada usuario ve únicamente sus
            propios datos.
          </Typography>
        </Box>

        {/* Paso 2 */}
        <Box sx={{ position: "relative" }}>
          <Box
            sx={{
              width: 40,
              height: 40,
              borderRadius: "50%",
              border: "1px solid #2A3B33",
              borderColor: "primary.main",
              bgcolor: "#12211B",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              fontWeight: 700,
              position: "relative",
              zIndex: 1,
            }}
          >
            2
          </Box>

          <Typography
            variant="h5"
            sx={{
              fontFamily: '"Sora", sans-serif',
              fontWeight: 700,
              mt: 3,
              mb: 1,
            }}
          >
            Sumá tus cuentas
          </Typography>

          <Typography variant="body1">
            Cargás banco, Mercado Pago y efectivo, cada una con su nombre y su
            moneda.
          </Typography>
        </Box>

        {/* Paso 3 */}
        <Box sx={{ position: "relative" }}>
          <Box
            sx={{
              width: 40,
              height: 40,
              borderRadius: "50%",
              border: "1px solid #2A3B33",
              borderColor: "primary.main",
              bgcolor: "#12211B",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              fontWeight: 700,
              position: "relative",
              zIndex: 1,
            }}
          >
            3
          </Box>

          <Typography
            variant="h5"
            sx={{
              fontFamily: '"Sora", sans-serif',
              fontWeight: 700,
              mt: 3,
              mb: 1,
            }}
          >
            Registrá y mirá
          </Typography>

          <Typography variant="body1">
            Cargás cada ingreso y cada gasto, y seguís el saldo desde el panel.
          </Typography>
        </Box>
      </Box>

      {/* Preguntas frecuentes */}
      <Container id="preguntas-frecuentes" maxWidth="md" sx={{ py: 6, scrollMarginTop: 10 }}>
        <Typography
          variant="h5"
          sx={{
            color: "grey",
            fontFamily: '"Sora", sans-serif',
            fontWeight: 700,
            mb: 2,
          }}
        >
          PREGUNTAS FRECUENTES
        </Typography>
        <Typography variant="h2" color="text.secondary" sx={{ fontSize: { xs: "2rem", md: "3.75rem" } }}>
          Lo que se pregunta <br /> antes de empezar
        </Typography>

        <Divider sx={{ my: 3 }} />

        <Typography
          variant="body1"
          sx={{ mb: 2, opacity: 0.85, fontWeight: 700 }}
        >
          ¿Qué necesito para empezar?
        </Typography>
        <Typography variant="body2" sx={{ mb: 2, opacity: 0.85 }}>
          Solo crearte un usuario. No hay que conectar el homebanking ni
          instalar nada: cargás tus cuentas y registrás los movimientos a mano,
          a medida que pasan.
        </Typography>

        <Divider sx={{ my: 3 }} />

        <Typography
          variant="body1"
          sx={{ mb: 2, opacity: 0.85, fontWeight: 700 }}
        >
          ¿Puedo tener cuentas en distintas monedas?
        </Typography>
        <Typography variant="body2" sx={{ mb: 2, opacity: 0.85 }}>
          Sí. Cada cuenta se crea con su nombre y su moneda, y Finova muestra el
          saldo de cada una en su propia moneda.{" "}
        </Typography>

        <Divider sx={{ my: 3 }} />

        <Typography
          variant="body1"
          sx={{ mb: 2, opacity: 0.85, fontWeight: 700 }}
        >
          ¿Quién puede ver mis datos?{" "}
        </Typography>
        <Typography variant="body2" sx={{ mb: 2, opacity: 0.85 }}>
          Solo vos. Cada usuario accede únicamente a sus propias cuentas y a sus
          propios movimientos.{" "}
        </Typography>

        <Divider sx={{ my: 3 }} />

        <Typography
          variant="body1"
          sx={{ mb: 2, opacity: 0.85, fontWeight: 700 }}
        >
          ¿Qué hay en cada versión?{" "}
        </Typography>
        <Typography variant="body2" sx={{ mb: 2, opacity: 0.85 }}>
          La V1 es el núcleo: cuentas, ingresos, gastos, historial y panel. La
          V2 agrega gráficos, comparaciones, presupuestos y metas de ahorro. La
          V3 suma el asistente que responde preguntas sobre tus datos.{" "}
        </Typography>

        <Divider sx={{ my: 3 }} />

        <Typography
          variant="body1"
          sx={{ mb: 2, opacity: 0.85, fontWeight: 700 }}
        >
          ¿Reemplaza a mi contador?{" "}
        </Typography>
        <Typography variant="body2" sx={{ mb: 2, opacity: 0.85 }}>
          No. Finova ordena tu día a día y te muestra en qué gastás. Para
          impuestos y presentaciones formales vas a seguir necesitando a tu
          contador.{" "}
        </Typography>
      </Container>

      {/* Empeza hoy */}
      <Container
        maxWidth="lg"
        sx={{
          pt: 1,
          py: 5,
        }}
      >
        <Box
          sx={{
            backgroundColor: "white",
            borderRadius: 3,
            overflow: "hidden",
            textAlign: "center",
            pb: 4,
            boxShadow: 2,
          }}
        >
          <Box
            sx={{
              display: "flex",
              justifyContent: "center",
              pt: 3,
            }}
          >
            <Typography
              variant="h5"
              sx={{
                fontFamily: '"Sora", sans-serif',
                fontWeight: 700,
                backgroundColor: "#c0c0c0",
                borderRadius: 3,
                px: 3,
                py: 1.5,
              }}
            >
              Empezá hoy
            </Typography>
          </Box>

          {/* Descripción */}
          <Typography
            variant="body1"
            sx={{
              opacity: 0.85,
              px: 1,
              mt: 3,
              mb: 3,
              color: "black",
            }}
          >
            Creá tu usuario, cargá tus cuentas y registrá tu primer movimiento.
            Desde ahí, todo lo que entra y sale queda en un solo lugar.
          </Typography>

          {/* Botón */}
          <Button
            variant="contained"
            onClick={() => go("/registro")}
            sx={{
              backgroundColor: "#000000",
              color: "#ffffff",
              textTransform: "none",
              fontWeight: 600,
              px: 4,
              py: 1.5,
              borderRadius: 2,
              "&:hover": {
                backgroundColor: "#333333",
              },
            }}
          >
            Pedir acceso
          </Button>
        </Box>
      </Container>

      {/* FOOTER */}
      <Box
        component="footer"
        sx={{
          bgcolor: "#030303",
          color: "#F4F6F2",
          borderTop: "1px solid #2A3B33",
          py: 6,
        }}
      >
        <Container maxWidth="lg">
          <Box
            sx={{
              display: "grid",
              gridTemplateColumns: { xs: "1fr", sm: "1fr 1fr", md: "1.5fr 1fr 1.2fr" },
              gap: 4,
            }}
          >
            {/* Marca */}
            <Box>
              <Image src="/Logo.png" alt="Finova" width={56} height={56} style={{ borderRadius: 12, height: 56, width: "auto" }} />
              <Typography variant="body2" sx={{ mt: 2, opacity: 0.75 }}>
                Tus finanzas, en un solo lugar.
              </Typography>
            </Box>

            {/* Links rápidos */}
            <Box sx={{ display: "flex", flexDirection: "column", gap: 1 }}>
              <Typography variant="subtitle2" sx={{ fontWeight: 700, mb: 0.5 }}>Finova</Typography>
              <Link href="#funcionalidades" underline="hover" sx={{ color: "inherit", opacity: 0.75 }}>
                Funcionalidades
              </Link>
              <Link href="#preguntas-frecuentes" underline="hover" sx={{ color: "inherit", opacity: 0.75 }}>
                Preguntas frecuentes
              </Link>
              <Link href="#contacto" underline="hover" sx={{ color: "inherit", opacity: 0.75 }}>
                Contacto
              </Link>
            </Box>

            {/* Contacto */}
            <Box id="contacto" sx={{ display: "flex", flexDirection: "column", gap: 1, scrollMarginTop: 10 }}>
              <Typography variant="subtitle2" sx={{ fontWeight: 700, mb: 0.5 }}>Contacto</Typography>
              <Link
                href="mailto:estevezgaston01@gmail.com"
                underline="hover"
                sx={{ color: "inherit", opacity: 0.75, display: "flex", alignItems: "center", gap: 1 }}
              >
                <FiMail aria-hidden /> estevezgaston01@gmail.com
              </Link>
              <Link
                href="https://www.linkedin.com/in/gastonestevez01"
                target="_blank"
                rel="noopener noreferrer"
                underline="hover"
                sx={{ color: "inherit", opacity: 0.75, display: "flex", alignItems: "center", gap: 1 }}
              >
                <FaLinkedin aria-hidden /> LinkedIn
              </Link>
              <Link
                href="https://github.com/GastonEE01"
                target="_blank"
                rel="noopener noreferrer"
                underline="hover"
                sx={{ color: "inherit", opacity: 0.75, display: "flex", alignItems: "center", gap: 1 }}
              >
                <FaGithub aria-hidden /> GitHub
              </Link>
            </Box>
          </Box>

          <Typography variant="caption" sx={{ display: "block", mt: 5, opacity: 0.55 }}>
            © 2026 Finova · Proyecto personal de Gastón Estevez
          </Typography>
        </Container>
      </Box>
    </Box>
    </ThemeProvider>
  );
}
