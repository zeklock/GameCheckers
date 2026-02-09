import type { ReactNode } from "react";

type PropsType = {
  onClick?: () => void;
  disabled?: boolean;
  className?: string;
  loading?: boolean;
  children: ReactNode;
};

export default function Button({
  onClick,
  disabled,
  className,
  loading,
  children,
}: PropsType) {
  return (
    <button
      onClick={onClick}
      disabled={disabled}
      className={`
        ${className}
        w-full py-4 px-6 rounded-xl font-semibold text-lg transition-all duration-200
        ${
          loading
            ? "bg-gray-600 cursor-not-allowed"
            : "bg-indigo-600 hover:bg-indigo-700 active:bg-indigo-800 shadow-lg shadow-indigo-500/30 hover:shadow-indigo-500/50"
        }
        text-white focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 focus:ring-offset-gray-900
        `}
    >
      {children}
    </button>
  );
}
