"use client";

import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { getToken } from "./lib/auth";
import BienvenidaPage from "./bienvenida/page";

export default function Home() {
  const router = useRouter();
  const [checked, setChecked] = useState(false);

  useEffect(() => {
    if (getToken()) {
      router.replace("/dashboard");
      return;
    }
    let cancelled = false;
    Promise.resolve().then(() => {
      if (!cancelled) setChecked(true);
    });
    return () => { cancelled = true; };
  }, [router]);

  if (!checked) return null;
  return <BienvenidaPage />;
}
