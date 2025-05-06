import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "Crossessor App",
  description: "Crossessor for ai compare",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="tr">
      <body
      >
        {children}
      </body>
    </html>
  );
}
